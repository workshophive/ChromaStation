using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Alert;
using Content.Shared.Bed.Sleep;
using Content.Shared.CCVar;
using Content.Shared.Friction;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.StepTrigger.Components;
using Content.Shared.Tag;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using PullableComponent = Content.Shared.Movement.Pulling.Components.PullableComponent;

namespace Content.Shared.Movement.Systems;

/// <summary>
///     Handles player and NPC mob movement.
///     NPCs are handled server-side only.
/// </summary>
public abstract partial class SharedMoverController : VirtualController
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tags = default!;

    protected EntityQuery<InputMoverComponent> MoverQuery;
    protected EntityQuery<MobMoverComponent> MobMoverQuery;
    protected EntityQuery<MovementRelayTargetComponent> RelayTargetQuery;
    protected EntityQuery<MovementSpeedModifierComponent> ModifierQuery;
    protected EntityQuery<PhysicsComponent> PhysicsQuery;
    protected EntityQuery<RelayInputMoverComponent> RelayQuery;
    protected EntityQuery<PullableComponent> PullableQuery;
    protected EntityQuery<TransformComponent> XformQuery;
    protected EntityQuery<CanMoveInAirComponent> CanMoveInAirQuery;
    protected EntityQuery<NoRotateOnMoveComponent> NoRotateQuery;
    protected EntityQuery<FootstepModifierComponent> FootstepModifierQuery;
    protected EntityQuery<MapGridComponent> MapGridQuery;

    /// <summary>
    /// <see cref="CCVars.StopSpeed"/>
    /// </summary>
    private float _stopSpeed;

    private bool _relativeMovement;

    /// <summary>
            return false;
        }

        mobMover.LastPosition = coordinates;

        if (mobMover.StepSoundDistance < distanceNeeded)
            return false;

        var soundEv = new MakeFootstepSoundEvent();
        RaiseLocalEvent(uid, soundEv);

        mobMover.StepSoundDistance -= distanceNeeded;

        if (FootstepModifierQuery.TryComp(uid, out var moverModifier))
        {
            sound = moverModifier.FootstepSoundCollection;
            return sound != null;
        }

        if (_entities.TryGetComponent(uid, out NoShoesSilentFootstepsComponent? _) &
            !_inventory.TryGetSlotEntity(uid, "shoes", out var _))
        {
            return false;
        }

        if (_inventory.TryGetSlotEntity(uid, "shoes", out var shoes) &&
            FootstepModifierQuery.TryComp(shoes, out var modifier))
        {
            sound = modifier.FootstepSoundCollection;
            return sound != null;
        }

        return TryGetFootstepSound(uid, xform, shoes != null, out sound, tileDef: tileDef);
    }

    private bool TryGetFootstepSound(
        EntityUid uid,
        TransformComponent xform,
        bool haveShoes,
        [NotNullWhen(true)] out SoundSpecifier? sound,
        ContentTileDefinition? tileDef = null)
    {
        sound = null;

        // Fallback to the map?
        if (!MapGridQuery.TryComp(xform.GridUid, out var grid))
        {
            if (FootstepModifierQuery.TryComp(xform.MapUid, out var modifier))
            {
                sound = modifier.FootstepSoundCollection;
            }

            return sound != null;
        }

        var position = grid.LocalToTile(xform.Coordinates);
        var soundEv = new GetFootstepSoundEvent(uid);

        // If the coordinates have a FootstepModifier component
        // i.e. component that emit sound on footsteps emit that sound
        var anchored = grid.GetAnchoredEntitiesEnumerator(position);

        while (anchored.MoveNext(out var maybeFootstep))
        {
            RaiseLocalEvent(maybeFootstep.Value, ref soundEv);

            if (soundEv.Sound != null)
            {
                sound = soundEv.Sound;
                return true;
            }

            if (FootstepModifierQuery.TryComp(maybeFootstep, out var footstep))
            {
                sound = footstep.FootstepSoundCollection;
                return sound != null;
            }
        }

        // Walking on a tile.
        // Tile def might have been passed in already from previous methods, so use that
        // if we have it
        if (tileDef == null && grid.TryGetTileRef(position, out var tileRef))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];
        }

        if (tileDef == null)
            return false;

        sound = haveShoes ? tileDef.FootstepSounds : tileDef.BarestepSounds;
        return sound != null;
    }

    private Vector2 AssertValidWish(InputMoverComponent mover, float walkSpeed, float sprintSpeed)
    {
        var (walkDir, sprintDir) = GetVelocityInput(mover);

        var total = walkDir * walkSpeed + sprintDir * sprintSpeed;

        var parentRotation = GetParentGridAngle(mover);
        var wishDir = _relativeMovement ? parentRotation.RotateVec(total) : total;

        DebugTools.Assert(MathHelper.CloseToPercent(total.Length(), wishDir.Length()));

        return wishDir;
    }

    private void OnTileFriction(Entity<MovementSpeedModifierComponent> ent, ref TileFrictionEvent args)
    {
        if (!TryComp<PhysicsComponent>(ent, out var physicsComponent) || !XformQuery.TryComp(ent, out var xform))
            return;

        if (physicsComponent.BodyStatus != BodyStatus.OnGround || _gravity.IsWeightless(ent.Owner))
            args.Modifier *= ent.Comp.BaseWeightlessFriction;
        else
            args.Modifier *= ent.Comp.BaseFriction;
    }
}
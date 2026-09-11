using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class RifleAnimatorSetup
{
    private const string AnimationFolder =
        "Assets/Animaton/Basic Shooter Pack/";

    private const string ControllerPath =
        "Assets/Animaton/Male Locomotion Pack/Player/Player.controller";

    private const string PlayerPrefabPath =
        "Assets/Prefabs/Player.prefab";

    private const string MaskPath =
        AnimationFolder + "Ch15RifleUpperBodyMask.mask";

    private const string RifleLayerName = "Rifle Layer";
    private const string RifleLocomotionStateName = "Rifle Locomotion";
    private const string RifleLocomotionTreeName = "Rifle Locomotion Blend Tree";
    private const string HasRifle = "HasRifle";
    private const string IsAiming = "IsAiming";
    private const string IsGrounded = "IsGrounded";
    private const string Fire = "Fire";
    private const string Reload = "Reload";

    private const string MigrationVersionKey =
        "TPS.RifleAnimatorSetup.BasicShooterPack.v4";

    [InitializeOnLoadMethod]
    private static void ScheduleBasicShooterMigration()
    {
        if (EditorPrefs.GetBool(MigrationVersionKey, false))
            return;

        EditorApplication.delayCall += ApplyBasicShooterMigration;
    }

    private static void ApplyBasicShooterMigration()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyBasicShooterMigration;
            return;
        }

        if (!AreBuildAssetsReady())
        {
            EditorApplication.delayCall += ApplyBasicShooterMigration;
            return;
        }

        Build();
        EditorPrefs.SetBool(MigrationVersionKey, true);
    }

    private static bool AreBuildAssetsReady()
    {
        return AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null &&
            LoadPlayerAnimator() != null &&
            LoadClip("rifle aiming idle.fbx") != null &&
            LoadClip("firing rifle.fbx") != null &&
            LoadClip("reloading.fbx") != null;
    }

    [MenuItem("Tools/TPS/Build Rifle Animator Layer")]
    public static void Build()
    {
        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        Animator playerAnimator =
            LoadPlayerAnimator();

        if (controller == null || playerAnimator == null)
        {
            Debug.LogError(
                "[Rifle Animation Setup] Player.controller or the Player prefab Animator is missing."
            );

            return;
        }

        ConfigureClipImport("rifle aiming idle.fbx", true);
        ConfigureClipImport("firing rifle.fbx", false);
        ConfigureClipImport("reloading.fbx", false);
        ConfigureClipImport("rifle run.fbx", true);
        ConfigureClipImport("run backwards.fbx", true);
        ConfigureClipImport("walking.fbx", true);
        ConfigureClipImport("walking backwards.fbx", true);
        ConfigureClipImport("strafe left.fbx", true);
        ConfigureClipImport("strafe right.fbx", true);

        AvatarMask mask =
            CreateOrUpdateMask(playerAnimator);

        AnimationClip aimClip =
            LoadClip("rifle aiming idle.fbx");

        AnimationClip reloadClip =
            LoadClip("reloading.fbx");

        if (mask == null ||
            aimClip == null ||
            reloadClip == null)
        {
            return;
        }

        EnsureParameter(controller, HasRifle, AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, IsAiming, AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, Fire, AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, Reload, AnimatorControllerParameterType.Trigger);

        BuildRifleBaseLocomotion(controller);

        int layerIndex =
            FindOrCreateLayer(controller);

        AnimatorControllerLayer layer =
            controller.layers[layerIndex];

        layer.avatarMask = mask;
        layer.blendingMode = AnimatorLayerBlendingMode.Override;
        layer.defaultWeight = 1f;
        controller.layers[layerIndex] = layer;

        AnimatorStateMachine stateMachine =
            layer.stateMachine;

        ClearStateMachine(stateMachine);

        AnimatorState emptyState =
            stateMachine.AddState("Empty", new Vector3(100f, 180f));

        AnimatorState aimState =
            stateMachine.AddState("Rifle Aim", new Vector3(600f, 180f));
        aimState.motion = aimClip;

        AnimatorState reloadState =
            stateMachine.AddState("Reload", new Vector3(600f, 360f));
        reloadState.motion = reloadClip;

        stateMachine.defaultState = emptyState;

        AddAnyStateBoolTransition(
            stateMachine,
            aimState,
            HasRifle,
            IsAiming
        );

        AddBoolTransition(aimState, emptyState, IsAiming, false, false);

        AddAnyStateTriggerTransition(
            stateMachine,
            reloadState,
            Reload,
            HasRifle
        );

        AddExitTransition(reloadState, aimState, IsAiming, true);
        AddExitTransition(reloadState, emptyState, IsAiming, false);

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(mask);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Rifle Animation Setup] Rifle locomotion added to Base Layer; firing data is retained without a visual shoot animation."
        );
    }

    private static void BuildRifleBaseLocomotion(
        AnimatorController controller)
    {
        AnimationClip idle =
            LoadClip("rifle aiming idle.fbx");

        AnimationClip walk =
            LoadClip("walking.fbx");

        AnimationClip walkBackward =
            LoadClip("walking backwards.fbx");

        AnimationClip run =
            LoadClip("rifle run.fbx");

        AnimationClip runBackward =
            LoadClip("run backwards.fbx");

        AnimationClip strafeLeft =
            LoadClip("strafe left.fbx");

        AnimationClip strafeRight =
            LoadClip("strafe right.fbx");

        if (new[]
            {
                idle,
                walk,
                walkBackward,
                run,
                runBackward,
                strafeLeft,
                strafeRight
            }.Any(clip => clip == null))
        {
            return;
        }

        AnimatorControllerLayer baseLayer =
            controller.layers[0];

        AnimatorStateMachine stateMachine =
            baseLayer.stateMachine;

        BlendTree rifleTree =
            AssetDatabase.LoadAllAssetsAtPath(ControllerPath)
                .OfType<BlendTree>()
                .FirstOrDefault(tree =>
                    tree.name == RifleLocomotionTreeName);

        if (rifleTree == null)
        {
            rifleTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(rifleTree, controller);
        }

        rifleTree.name = RifleLocomotionTreeName;
        rifleTree.blendType = BlendTreeType.FreeformCartesian2D;
        rifleTree.blendParameter = "MoveX";
        rifleTree.blendParameterY = "MoveY";
        rifleTree.useAutomaticThresholds = false;
        rifleTree.children =
            new[]
            {
                CreateMotion(idle, Vector2.zero),
                CreateMotion(walk, new Vector2(0f, 0.5f)),
                CreateMotion(walkBackward, new Vector2(0f, -0.5f)),
                CreateMotion(strafeLeft, new Vector2(-0.5f, 0f)),
                CreateMotion(strafeRight, new Vector2(0.5f, 0f)),
                CreateMotion(run, new Vector2(0f, 1f)),
                CreateMotion(runBackward, new Vector2(0f, -1f)),
                CreateMotion(strafeLeft, new Vector2(-1f, 0f)),
                CreateMotion(strafeRight, new Vector2(1f, 0f))
            };

        AnimatorState normalState =
            FindState(stateMachine, "Locomotion");

        AnimatorState rifleState =
            FindState(stateMachine, RifleLocomotionStateName);

        if (normalState == null)
        {
            Debug.LogError(
                "[Rifle Animation Setup] Base Locomotion state was not found."
            );

            return;
        }

        if (rifleState == null)
        {
            rifleState =
                stateMachine.AddState(
                    RifleLocomotionStateName,
                    new Vector3(620f, 50f)
                );
        }

        rifleState.motion = rifleTree;

        EnsureBoolTransition(
            normalState,
            rifleState,
            HasRifle,
            true
        );

        EnsureBoolTransition(
            rifleState,
            normalState,
            HasRifle,
            false
        );

        AnimatorState jumpState =
            FindState(stateMachine, "Jump");

        if (jumpState != null)
        {
            AddConditionIfMissing(
                FindTransition(
                    jumpState,
                    normalState,
                    IsGrounded
                ),
                AnimatorConditionMode.IfNot,
                HasRifle
            );

            EnsureGroundedTransition(
                jumpState,
                rifleState
            );
        }

        EditorUtility.SetDirty(rifleTree);
        EditorUtility.SetDirty(stateMachine);
    }

    private static AnimatorState FindState(
        AnimatorStateMachine stateMachine,
        string stateName)
    {
        return stateMachine.states
            .Select(child => child.state)
            .FirstOrDefault(state => state.name == stateName);
    }

    private static void EnsureBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string parameter,
        bool expectedValue)
    {
        if (FindTransition(from, to, parameter) != null)
            return;

        AddBoolTransition(
            from,
            to,
            parameter,
            expectedValue,
            false
        );
    }

    private static AnimatorStateTransition FindTransition(
        AnimatorState from,
        AnimatorState to,
        string parameter)
    {
        return from.transitions.FirstOrDefault(transition =>
            transition.destinationState == to &&
            transition.conditions.Any(condition =>
                condition.parameter == parameter));
    }

    private static void AddConditionIfMissing(
        AnimatorStateTransition transition,
        AnimatorConditionMode mode,
        string parameter)
    {
        if (transition == null ||
            transition.conditions.Any(condition =>
                condition.parameter == parameter))
        {
            return;
        }

        transition.AddCondition(mode, 0f, parameter);
    }

    private static void EnsureGroundedTransition(
        AnimatorState from,
        AnimatorState to)
    {
        AnimatorStateTransition transition =
            from.transitions.FirstOrDefault(candidate =>
                candidate.destinationState == to &&
                candidate.conditions.Any(condition =>
                    condition.parameter == IsGrounded));

        if (transition != null)
            return;

        transition =
            from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            IsGrounded
        );
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            HasRifle
        );
    }

    private static void AddAnyStateBoolTransition(
        AnimatorStateMachine stateMachine,
        AnimatorState target,
        string firstBool,
        string secondBool)
    {
        AnimatorStateTransition transition =
            stateMachine.anyStateTransitions.FirstOrDefault(candidate =>
                candidate.destinationState == target &&
                candidate.conditions.Any(condition =>
                    condition.parameter == firstBool) &&
                candidate.conditions.Any(condition =>
                    condition.parameter == secondBool));

        if (transition != null)
            return;

        transition =
            stateMachine.AddAnyStateTransition(target);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            firstBool
        );
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            secondBool
        );
    }

    private static ChildMotion CreateMotion(
        Motion motion,
        Vector2 position)
    {
        return new ChildMotion
        {
            motion = motion,
            position = position,
            timeScale = 1f,
            cycleOffset = 0f,
            mirror = false,
            directBlendParameter = string.Empty
        };
    }

    private static Animator LoadPlayerAnimator()
    {
        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

        return prefab != null
            ? prefab.GetComponentInChildren<Animator>(true)
            : null;
    }

    private static AvatarMask CreateOrUpdateMask(Animator animator)
    {
        AvatarMask mask =
            AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);

        if (mask == null)
        {
            mask = new AvatarMask();
            AssetDatabase.CreateAsset(mask, MaskPath);
        }

        List<string> paths =
            new List<string>();

        HumanBodyBones[] upperBodyBones =
        {
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand
        };

        foreach (HumanBodyBones bone in upperBodyBones)
        {
            Transform boneTransform =
                animator.GetBoneTransform(bone);

            if (boneTransform == null)
                continue;

            string path =
                AnimationUtility.CalculateTransformPath(
                    boneTransform,
                    animator.transform
                );

            if (!paths.Contains(path))
                paths.Add(path);
        }

        if (paths.Count == 0)
        {
            Debug.LogError(
                "[Rifle Animation Setup] No humanoid upper-body bones were found for the Avatar Mask."
            );

            return null;
        }

        mask.transformCount = paths.Count;

        for (int index = 0; index < paths.Count; index++)
        {
            mask.SetTransformPath(index, paths[index]);
            mask.SetTransformActive(index, true);
        }

        return mask;
    }

    private static int FindOrCreateLayer(
        AnimatorController controller)
    {
        for (int index = 0; index < controller.layers.Length; index++)
        {
            if (controller.layers[index].name == RifleLayerName)
                return index;
        }

        controller.AddLayer(RifleLayerName);
        return controller.layers.Length - 1;
    }

    private static void AddBoolTransition(
        AnimatorState from,
        AnimatorState to,
        string parameter,
        bool expectedValue,
        bool hasExitTime)
    {
        AnimatorStateTransition transition =
            from.AddTransition(to);

        transition.hasExitTime = hasExitTime;
        transition.exitTime = 0.9f;
        transition.duration = 0.1f;
        transition.AddCondition(
            expectedValue
                ? AnimatorConditionMode.If
                : AnimatorConditionMode.IfNot,
            0f,
            parameter
        );
    }

    private static void AddAnyStateTriggerTransition(
        AnimatorStateMachine stateMachine,
        AnimatorState target,
        string trigger,
        string requiredBool)
    {
        AnimatorStateTransition transition =
            stateMachine.AddAnyStateTransition(target);

        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            requiredBool
        );
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            trigger
        );
    }

    private static void AddExitTransition(
        AnimatorState from,
        AnimatorState to,
        string parameter,
        bool expectedValue)
    {
        AnimatorStateTransition transition =
            from.AddTransition(to);

        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.1f;
        transition.AddCondition(
            expectedValue
                ? AnimatorConditionMode.If
                : AnimatorConditionMode.IfNot,
            0f,
            parameter
        );
    }

    private static AnimationClip LoadClip(string fileName)
    {
        string path = AnimationFolder + fileName;
        AnimationClip clip =
            AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate =>
                    !candidate.name.StartsWith("__preview__"));

        if (clip == null)
        {
            Debug.LogError(
                $"[Rifle Animation Setup] Animation clip not found at {path}."
            );
        }

        return clip;
    }

    private static void ConfigureClipImport(
        string fileName,
        bool shouldLoop)
    {
        string path = AnimationFolder + fileName;
        ModelImporter importer =
            AssetImporter.GetAtPath(path) as ModelImporter;

        if (importer == null)
        {
            Debug.LogError(
                $"[Rifle Animation Setup] Model importer not found at {path}."
            );

            return;
        }

        ModelImporterClipAnimation[] clips =
            importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;

        bool changed = false;

        for (int index = 0; index < clips.Length; index++)
        {
            if (clips[index].loopTime == shouldLoop &&
                clips[index].lockRootRotation &&
                clips[index].lockRootHeightY &&
                clips[index].lockRootPositionXZ)
            {
                continue;
            }

            clips[index].loopTime = shouldLoop;
            clips[index].lockRootRotation = true;
            clips[index].lockRootHeightY = true;
            clips[index].lockRootPositionXZ = true;
            changed = true;
        }

        if (!changed)
            return;

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static void EnsureParameter(
        AnimatorController controller,
        string parameterName,
        AnimatorControllerParameterType parameterType)
    {
        AnimatorControllerParameter existing =
            controller.parameters.FirstOrDefault(parameter =>
                parameter.name == parameterName);

        if (existing == null)
        {
            controller.AddParameter(parameterName, parameterType);
            return;
        }

        if (existing.type == parameterType)
            return;

        controller.RemoveParameter(existing);
        controller.AddParameter(parameterName, parameterType);
    }

    private static void ClearStateMachine(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
            stateMachine.RemoveState(childState.state);

        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            stateMachine.RemoveStateMachine(childStateMachine.stateMachine);

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            stateMachine.RemoveAnyStateTransition(transition);

        foreach (AnimatorTransition transition in stateMachine.entryTransitions)
            stateMachine.RemoveEntryTransition(transition);
    }
}

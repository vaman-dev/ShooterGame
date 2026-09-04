using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerBaseAnimatorSetup
{
    private const string RepairVersionKey =
        "TPS.PlayerAnimationRepair.v1";

    private const string AnimationFolder =
        "Assets/Animaton/Male Locomotion Pack/Player/";

    private const string ControllerPath =
        AnimationFolder + "Player.controller";

    private const string PlayerPrefabPath =
        "Assets/Prefabs/Player.prefab";

    private const string MoveX = "MoveX";
    private const string MoveY = "MoveY";
    private const string MoveAmount = "MoveAmount";
    private const string IsSprinting = "IsSprinting";
    private const string IsGrounded = "IsGrounded";
    private const string VerticalVelocity = "VerticalVelocity";
    private const string Jump = "Jump";

    [InitializeOnLoadMethod]
    private static void SchedulePendingRepair()
    {
        if (EditorPrefs.GetBool(RepairVersionKey, false))
        {
            return;
        }

        EditorApplication.delayCall += ApplyPendingRepair;
    }

    private static void ApplyPendingRepair()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyPendingRepair;
            return;
        }

        Build();
        EditorPrefs.SetBool(RepairVersionKey, true);
    }

    [MenuItem("Tools/TPS/Build Base Player Animator")]
    public static void Build()
    {
        ConfigureClipImport("idle.fbx", true);
        ConfigureClipImport("walking.fbx", true);
        ConfigureClipImport("standard run.fbx", true);
        ConfigureClipImport("left strafe walking.fbx", true);
        ConfigureClipImport("right strafe walking.fbx", true);
        ConfigureClipImport("left strafe.fbx", true);
        ConfigureClipImport("right strafe.fbx", true);
        ConfigureClipImport("jump.fbx", false);
        ConfigureClipImport("left turn 90.fbx", false);
        ConfigureClipImport("right turn 90.fbx", false);

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        if (controller == null)
        {
            Debug.LogError(
                $"[Animation Setup] Animator Controller not found at {ControllerPath}."
            );

            return;
        }

        EnsureParameter(controller, MoveX, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, MoveY, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, MoveAmount, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, IsSprinting, AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, IsGrounded, AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, VerticalVelocity, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, Jump, AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;

        ClearStateMachine(stateMachine);

        BlendTree locomotionTree =
            AssetDatabase.LoadAllAssetsAtPath(ControllerPath)
                .OfType<BlendTree>()
                .FirstOrDefault();

        if (locomotionTree == null)
        {
            locomotionTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(locomotionTree, controller);
        }

        locomotionTree.name = "Locomotion Blend Tree";
        locomotionTree.blendType = BlendTreeType.FreeformCartesian2D;
        locomotionTree.blendParameter = MoveX;
        locomotionTree.blendParameterY = MoveY;
        locomotionTree.useAutomaticThresholds = false;

        AnimationClip idle = LoadClip("idle.fbx");
        AnimationClip walk = LoadClip("walking.fbx");
        AnimationClip run = LoadClip("standard run.fbx");
        AnimationClip walkLeft = LoadClip("left strafe walking.fbx");
        AnimationClip walkRight = LoadClip("right strafe walking.fbx");
        AnimationClip runLeft = LoadClip("left strafe.fbx");
        AnimationClip runRight = LoadClip("right strafe.fbx");
        AnimationClip jump = LoadClip("jump.fbx");

        if (new[]
            {
                idle,
                walk,
                run,
                walkLeft,
                walkRight,
                runLeft,
                runRight,
                jump
            }.Any(clip => clip == null))
        {
            Object.DestroyImmediate(locomotionTree, true);

            Debug.LogError(
                "[Animation Setup] One or more required animation clips could not be loaded."
            );

            return;
        }

        locomotionTree.children =
            new[]
            {
                CreateMotion(idle, Vector2.zero),
                CreateMotion(walk, new Vector2(0f, 0.5f), 1.1f),
                CreateMotion(walk, new Vector2(0f, -0.5f), -1.1f),
                CreateMotion(walkLeft, new Vector2(-0.5f, 0f), 1.1f),
                CreateMotion(walkRight, new Vector2(0.5f, 0f), 1.1f),
                CreateMotion(run, new Vector2(0f, 1f), 1.25f),
                CreateMotion(run, new Vector2(0f, -1f), -1.25f),
                CreateMotion(runLeft, new Vector2(-1f, 0f), 1.25f),
                CreateMotion(runRight, new Vector2(1f, 0f), 1.25f)
            };

        AnimatorState locomotionState =
            stateMachine.AddState("Locomotion", new Vector3(300f, 180f));

        locomotionState.motion = locomotionTree;
        stateMachine.defaultState = locomotionState;

        AnimatorState jumpState =
            stateMachine.AddState("Jump", new Vector3(300f, 20f));

        jumpState.motion = jump;

        AnimatorStateTransition jumpTransition =
            stateMachine.AddAnyStateTransition(jumpState);

        jumpTransition.hasExitTime = false;
        jumpTransition.duration = 0.08f;
        jumpTransition.canTransitionToSelf = false;
        jumpTransition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            Jump
        );

        AnimatorStateTransition landTransition =
            jumpState.AddTransition(locomotionState);

        landTransition.hasExitTime = false;
        landTransition.exitTime = 0f;
        landTransition.duration = 0.1f;
        landTransition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            IsGrounded
        );

        AttachControllerToPlayerPrefab(controller);

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(locomotionTree);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Animation Setup] Base locomotion, responsive landing, baked root transforms, and Player prefab wiring completed."
        );
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
                $"[Animation Setup] Model importer not found for {path}."
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
                clips[index].lockRootPositionXZ &&
                !clips[index].mirror)
            {
                continue;
            }

            clips[index].loopTime = shouldLoop;
            clips[index].lockRootRotation = true;
            clips[index].lockRootHeightY = true;
            clips[index].lockRootPositionXZ = true;
            clips[index].mirror = false;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
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
                $"[Animation Setup] Animation clip not found in {path}."
            );
        }

        return clip;
    }

    private static ChildMotion CreateMotion(
        Motion motion,
        Vector2 position,
        float timeScale = 1f)
    {
        return new ChildMotion
        {
            motion = motion,
            position = position,
            timeScale = timeScale,
            cycleOffset = 0f,
            mirror = false,
            directBlendParameter = string.Empty
        };
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
        {
            return;
        }

        controller.RemoveParameter(existing);
        controller.AddParameter(parameterName, parameterType);
    }

    private static void ClearStateMachine(
        AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            stateMachine.RemoveState(childState.state);
        }

        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
        {
            stateMachine.RemoveStateMachine(childStateMachine.stateMachine);
        }

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }

        foreach (AnimatorTransition transition in stateMachine.entryTransitions)
        {
            stateMachine.RemoveEntryTransition(transition);
        }
    }

    private static void AttachControllerToPlayerPrefab(
        RuntimeAnimatorController controller)
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

        try
        {
            Animator animator =
                prefabRoot.GetComponentInChildren<Animator>(true);

            if (animator == null)
            {
                Debug.LogError(
                    "[Animation Setup] Player prefab does not contain an Animator."
                );

                return;
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            PlayerAnimationController animationController =
                prefabRoot.GetComponent<PlayerAnimationController>();

            if (animationController == null)
            {
                animationController =
                    prefabRoot.AddComponent<PlayerAnimationController>();
            }

            SerializedObject serializedController =
                new SerializedObject(animationController);

            serializedController.FindProperty("animator").objectReferenceValue = animator;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}

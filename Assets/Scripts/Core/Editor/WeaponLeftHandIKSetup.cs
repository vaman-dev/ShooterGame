using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;

public static class WeaponLeftHandIKSetup
{
    private const string VersionKey = "TPS.WeaponLeftHandIKSetup.v5";

    [InitializeOnLoadMethod]
    private static void ScheduleSetup()
    {
        if (EditorPrefs.GetBool(VersionKey, false))
            return;

        EditorApplication.delayCall += ApplySetup;
    }

    [MenuItem("Tools/TPS/Setup Rifle Left Hand IK")]
    public static void ApplySetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplySetup;
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject player = GameObject.Find("PlayerRoot");
        if (!scene.IsValid() || player == null)
        {
            Debug.LogError("[Rifle IK] An active scene PlayerRoot was not found.");
            return;
        }

        if (!BuildRig(player))
        {
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorPrefs.SetBool(VersionKey, true);
        Debug.Log("[Rifle IK] Created and wired the rifle left-hand IK rig on the active scene PlayerRoot.");
    }

    private static bool BuildRig(GameObject player)
    {
        Animator animator = FindAnimationOwner(player);
        WeaponAttachmentController attachment = player.GetComponent<WeaponAttachmentController>();
        WeaponLeftHandIKController controller = player.GetComponent<WeaponLeftHandIKController>();

        if (animator == null || attachment == null)
        {
            Debug.LogError("[Rifle IK] Player Animator or WeaponAttachmentController is missing.");
            return false;
        }

        if (controller == null)
            controller = Undo.AddComponent<WeaponLeftHandIKController>(player);

        Transform rigRoot = player.transform.Find("Rig");
        if (rigRoot == null)
            rigRoot = animator.transform.Find("Rig");

        if (rigRoot == null)
            rigRoot = GetOrCreate(animator.transform, "Rig");
        else if (rigRoot.parent != animator.transform)
            rigRoot.SetParent(animator.transform, false);

        Transform weaponRigRoot = GetOrCreate(rigRoot, "WeaponRig");
        Transform target = GetOrCreate(weaponRigRoot, "LeftHandIK_Target");
        Transform hint = GetOrCreate(weaponRigRoot, "LeftElbowHint");
        Transform constraintObject = GetOrCreate(weaponRigRoot, "LeftHandIK");

        Rig rig = weaponRigRoot.GetComponent<Rig>();
        if (rig == null)
            rig = Undo.AddComponent<Rig>(weaponRigRoot.gameObject);

        TwoBoneIKConstraint constraint = constraintObject.GetComponent<TwoBoneIKConstraint>();
        if (constraint == null)
            constraint = Undo.AddComponent<TwoBoneIKConstraint>(constraintObject.gameObject);

        Transform root = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform mid = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        Transform tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);

        if (root == null || mid == null || tip == null)
        {
            Debug.LogError("[Rifle IK] Ch15 left arm bones could not be resolved.");
            return false;
        }

        if (hint.localPosition == Vector3.zero && hint.localRotation == Quaternion.identity)
            hint.position = mid.position + (mid.position - tip.position).normalized * 0.25f;

        constraint.data.root = root;
        constraint.data.mid = mid;
        constraint.data.tip = tip;
        constraint.data.target = target;
        constraint.data.hint = hint;
        constraint.data.targetPositionWeight = 1f;
        constraint.data.targetRotationWeight = 1f;
        constraint.data.hintWeight = 1f;
        constraint.weight = 0f;

        RigBuilder rootBuilder = player.GetComponent<RigBuilder>();
        RigBuilder builder = animator.GetComponent<RigBuilder>();
        if (builder == null)
            builder = Undo.AddComponent<RigBuilder>(animator.gameObject);

        if (rootBuilder != null && rootBuilder != builder)
        {
            foreach (RigLayer layer in rootBuilder.layers)
            {
                if (layer.rig != null && !ContainsRig(builder, layer.rig))
                    builder.layers.Add(new RigLayer(layer.rig, layer.active));
            }

            Undo.DestroyObjectImmediate(rootBuilder);
        }

        if (!ContainsRig(builder, rig))
            builder.layers.Add(new RigLayer(rig));

        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("weaponAttachmentController").objectReferenceValue = attachment;
        serialized.FindProperty("weaponRig").objectReferenceValue = rig;
        serialized.FindProperty("leftHandIK").objectReferenceValue = constraint;
        serialized.FindProperty("leftHandIKTarget").objectReferenceValue = target;
        serialized.FindProperty("leftElbowHint").objectReferenceValue = hint;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(rig);
        EditorUtility.SetDirty(constraint);
        EditorUtility.SetDirty(builder);
        EditorUtility.SetDirty(controller);
        return true;
    }

    private static Animator FindAnimationOwner(GameObject player)
    {
        Animator[] animators = player.GetComponentsInChildren<Animator>(true);
        foreach (Animator candidate in animators)
        {
            if (candidate.runtimeAnimatorController != null &&
                candidate.avatar != null)
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool ContainsRig(RigBuilder builder, Rig rig)
    {
        foreach (RigLayer layer in builder.layers)
        {
            if (layer.rig == rig)
                return true;
        }

        return false;
    }

    private static Transform GetOrCreate(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing;

        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create Rifle IK Object");
        child.transform.SetParent(parent, false);
        return child.transform;
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WeaponAttachmentSetup
{
    private const string WeaponSocketProperty = "weaponSocket";
    [InitializeOnLoadMethod]
    private static void ScheduleInitialSetup()
    {
        EditorApplication.delayCall += ApplyInitialSetup;
    }

    private static void ApplyInitialSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyInitialSetup;
            return;
        }

        GameObject playerRoot =
            GameObject.Find("PlayerRoot");

        if (playerRoot == null)
        {
            return;
        }

        WeaponAttachmentController attachmentController =
            playerRoot.GetComponent<WeaponAttachmentController>();

        if (attachmentController == null)
        {
            return;
        }

        SerializedObject serializedController =
            new SerializedObject(attachmentController);
        SerializedProperty socketProperty =
            serializedController.FindProperty(WeaponSocketProperty);

        if (socketProperty != null &&
            socketProperty.objectReferenceValue != null)
        {
            return;
        }

        CreateWeaponSocket();
    }

    [MenuItem("Tools/TPS/Create Weapon Socket")]
    public static void CreateWeaponSocket()
    {
        Scene scene =
            SceneManager.GetActiveScene();

        if (!scene.IsValid() ||
            string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError(
                "[Weapon Attachment] An editable scene must be active."
            );

            return;
        }

        GameObject playerRoot =
            GameObject.Find("PlayerRoot");

        if (playerRoot == null)
        {
            Debug.LogError(
                "[Weapon Attachment] PlayerRoot was not found in the active scene."
            );

            return;
        }

        WeaponAttachmentController attachmentController =
            playerRoot.GetComponent<WeaponAttachmentController>();

        if (attachmentController == null)
        {
            Debug.LogError(
                "[Weapon Attachment] WeaponAttachmentController is missing from PlayerRoot."
            );

            return;
        }

        Animator animator =
            playerRoot.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogError(
                "[Weapon Attachment] PlayerRoot does not contain an Animator."
            );

            return;
        }

        Transform rightHand =
            animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (rightHand == null)
        {
            Debug.LogError(
                "[Weapon Attachment] Humanoid RightHand bone was not found."
            );

            return;
        }

        Transform weaponHolder =
            rightHand.Find("WeaponHolder");

        if (weaponHolder == null)
        {
            weaponHolder =
                new GameObject("WeaponHolder").transform;
            Undo.RegisterCreatedObjectUndo(
                weaponHolder.gameObject,
                "Create WeaponHolder"
            );
            weaponHolder.SetParent(rightHand, false);
        }

        Transform weaponSocket =
            weaponHolder.Find("WeaponSocket");

        if (weaponSocket == null)
        {
            weaponSocket =
                new GameObject("WeaponSocket").transform;
            Undo.RegisterCreatedObjectUndo(
                weaponSocket.gameObject,
                "Create WeaponSocket"
            );
            weaponSocket.SetParent(weaponHolder, false);
        }

        SerializedObject serializedController =
            new SerializedObject(attachmentController);
        SerializedProperty socketProperty =
            serializedController.FindProperty(WeaponSocketProperty);

        if (socketProperty == null)
        {
            Debug.LogError(
                "[Weapon Attachment] WeaponSocket field was not found on the attachment component."
            );

            return;
        }

        socketProperty.objectReferenceValue = weaponSocket;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(attachmentController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeTransform = weaponSocket;

        Debug.Log(
            "[Weapon Attachment] Created and wired PlayerRoot/Visual/.../RightHand/WeaponHolder/WeaponSocket."
        );
    }
}

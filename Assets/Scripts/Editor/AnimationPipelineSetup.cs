using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FoodSurvivors.Boss;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.EditorTools
{
    public static class AnimationPipelineSetup
    {
        private const string PlayerControllerPath = "Assets/Animations/Controllers/AC_Player.controller";
        private const string CourierControllerPath = "Assets/Animations/Controllers/AC_Boss_Courier.controller";
        private const string CartControllerPath = "Assets/Animations/Controllers/AC_Boss_Cart.controller";
        private const string EnemyControllerPath = "Assets/Animations/Controllers/AC_Enemy_Tourist.controller";
        private const string RegistryFolder = "Assets/Resources/FoodSurvivors";
        private const string RegistryPath = RegistryFolder + "/CharacterPrefabRegistry.asset";
        private const string GameScenePath = "Assets/Scenes/GameScene.unity";

        [MenuItem("FoodSurvivors/Setup All Animations And Prefabs")]
        public static void SetupAll()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Avatar charactersAvatar = FindCharactersAvatar();
            if (charactersAvatar == null)
            {
                Debug.LogError("[AnimationPipelineSetup] Could not find a valid humanoid Avatar named CharactersAvatar, or in Assets/PolygonShops/Models/Characters.fbx. Setup stopped before changing assets.");
                return;
            }

            ImportAnimationClips(charactersAvatar);
            AnimatorController playerController = LoadOrCreateController(PlayerControllerPath);
            AnimatorController courierController = LoadOrCreateController(CourierControllerPath);
            AnimatorController cartController = LoadOrCreateController(CartControllerPath);
            AnimatorController enemyController = LoadOrCreateController(EnemyControllerPath);

            ConfigurePlayerController(playerController);
            ConfigureSingleStateController(courierController, "Run", "Assets/Animations/Clips/Bosses/Anim_Boss_Courier_Run.fbx");
            ConfigureSingleStateController(cartController, "Idle", "Assets/Animations/Clips/Bosses/Anim_Boss_Cart_Idle.fbx");
            ConfigureEnemyController(enemyController);

            SetupPlayerPrefab(charactersAvatar, playerController);
            SetupBossPrefab("Assets/Prefabs/Characters/Bosses/TripleDelivery.prefab", charactersAvatar, courierController, null);
            SetupBossPrefab("Assets/Prefabs/Characters/Bosses/FoodCart.prefab", charactersAvatar, cartController, "SM_Chr_Santa_01");
            SetupEnemyPrefabs(charactersAvatar, enemyController);

            CharacterPrefabRegistry registry = CreateOrUpdateRegistry();
            if (registry == null)
            {
                Debug.LogError("[AnimationPipelineSetup] Could not create the runtime character prefab registry.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetupGameScenePlayer(registry.playerPrefab);
            Debug.Log("[AnimationPipelineSetup] Animation controllers, character prefabs, runtime prefab registry and GameScene player setup completed.");
        }

        private static Avatar FindCharactersAvatar()
        {
            // 1. Ищем аватар прямо на префабе Игрока или Санты
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Player/Player.prefab");
            if (playerPrefab != null)
            {
                Animator anim = playerPrefab.GetComponentInChildren<Animator>();
                if (anim != null && anim.avatar != null && anim.avatar.isHuman)
                {
                    Debug.Log($"[AnimationPipelineSetup] ✅ Аватар взят из Player.prefab: {anim.avatar.name}");
                    return anim.avatar;
                }
            }

            // 2. Ищем любой валидный Humanoid аватар в проекте
            string[] guids = AssetDatabase.FindAssets("t:Avatar");
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(p))
                {
                    Avatar av = obj as Avatar;
                    if (av != null && av.isValid && av.isHuman)
                    {
                        Debug.Log($"[AnimationPipelineSetup] ✅ Найден Humanoid аватар: {av.name} в {p}");
                        return av;
                    }
                }
            }

            // 3. Резерв: создаем из первой попавшейся анимации
            string[] animGuids = AssetDatabase.FindAssets("Anim_Player_Idle t:GameObject");
            if (animGuids.Length > 0)
            {
                string p = AssetDatabase.GUIDToAssetPath(animGuids[0]);
                foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(p))
                {
                    Avatar av = obj as Avatar;
                    if (av != null && av.isValid) return av;
                }
            }

            return null;
        }

        private static void ImportAnimationClips(Avatar avatar)
        {
            string[] fbxPaths = Directory.GetFiles("Assets/Animations/Clips", "*.fbx", SearchOption.AllDirectories)
                .Select(path => path.Replace('\\', '/')).OrderBy(path => path).ToArray();
            if (fbxPaths.Length != 13)
                Debug.LogWarning("[AnimationPipelineSetup] Expected 13 animation FBX files, found " + fbxPaths.Length + ". Importing all FBX files found under Assets/Animations/Clips.");

            foreach (string path in fbxPaths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    Debug.LogError("[AnimationPipelineSetup] ModelImporter not found for " + path);
                    continue;
                }

                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = avatar;

                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].loopTime = true;
                    clips[i].loopPose = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        private static AnimatorController LoadOrCreateController(string path)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller != null) return controller;

            string folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                string name = Path.GetFileName(folder);
                if (!AssetDatabase.IsValidFolder(parent)) Directory.CreateDirectory(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            return controller;
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(candidate => candidate != null && !candidate.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase));
            if (clip == null) Debug.LogError("[AnimationPipelineSetup] No imported AnimationClip found in " + path);
            return clip;
        }

        private static void ClearController(AnimatorController controller, string parameterName)
        {
            if (controller == null) return;
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            foreach (ChildAnimatorState child in stateMachine.states.ToArray())
                stateMachine.RemoveState(child.state);

            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions.ToArray())
                stateMachine.RemoveAnyStateTransition(transition);

            // Безопасное удаление entry переходов
            foreach (var transition in stateMachine.entryTransitions)
            {
                stateMachine.RemoveEntryTransition(transition);
            }

            foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
                controller.RemoveParameter(parameter);

            if (!string.IsNullOrEmpty(parameterName))
            {
                AnimatorControllerParameterType type = parameterName == "State" ? AnimatorControllerParameterType.Int : AnimatorControllerParameterType.Bool;
                controller.AddParameter(parameterName, type);
            }
            EditorUtility.SetDirty(controller);
        }

        private static AnimatorState AddState(AnimatorController controller, string name, AnimationClip clip, bool defaultState)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState state = stateMachine.AddState(name);
            state.motion = clip;
            if (defaultState) stateMachine.defaultState = state;
            return state;
        }

        private static void ConfigurePlayerController(AnimatorController controller)
        {
            ClearController(controller, "IsWalking");
            AnimatorState idle = AddState(controller, "Idle", LoadClip("Assets/Animations/Clips/Player/Anim_Player_Idle.fbx"), true);
            AnimatorState run = AddState(controller, "Run", LoadClip("Assets/Animations/Clips/Player/Anim_Player_Run.fbx"), false);
            AnimatorStateTransition startRun = idle.AddTransition(run);
            startRun.hasExitTime = false;
            startRun.duration = 0.15f;
            startRun.AddCondition(AnimatorConditionMode.If, 0f, "IsWalking");
            AnimatorStateTransition stopRun = run.AddTransition(idle);
            stopRun.hasExitTime = false;
            stopRun.duration = 0.15f;
            stopRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsWalking");
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureSingleStateController(AnimatorController controller, string stateName, string clipPath)
        {
            ClearController(controller, null);
            AddState(controller, stateName, LoadClip(clipPath), true);
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureEnemyController(AnimatorController controller)
        {
            ClearController(controller, "State");
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState run = AddState(controller, "Run", LoadClip("Assets/Animations/Clips/Enemies/Anim_Tourist_Run_1.fbx"), true);
            AnimatorState idle = AddState(controller, "Idle", LoadClip("Assets/Animations/Clips/Enemies/Anim_Tourist_Idle_1.fbx"), false);
            AnimatorState walk = AddState(controller, "Walk", LoadClip("Assets/Animations/Clips/Enemies/Anim_Tourist_Walk_2.fbx"), false);
            AddAnyStateTransition(stateMachine, run, 0);
            AddAnyStateTransition(stateMachine, idle, 1);
            AddAnyStateTransition(stateMachine, walk, 2);
            EditorUtility.SetDirty(controller);
        }

        private static void AddAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState state, int value)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = 0.2f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.Equals, value, "State");
        }

        private static void SetupPlayerPrefab(Avatar avatar, AnimatorController controller)
        {
            const string path = "Assets/Prefabs/Characters/Player/Player.prefab";
            EditPrefab(path, root =>
            {
                root.tag = "Player";
                ConfigureAnimator(root, avatar, controller, null);
                AddIfMissing<PlayerController>(root);
                AddIfMissing<PlayerVisuals>(root);
                AddIfMissing<StaminaSystem>(root);
                AddIfMissing<WeaponManager>(root);
                AddIfMissing<PassiveManager>(root);
                AddIfMissing<DamageFlash>(root);
                Rigidbody body = AddIfMissing<Rigidbody>(root);
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                CapsuleCollider capsule = AddIfMissing<CapsuleCollider>(root);
                capsule.direction = 1;
                if (capsule.height < 1.5f) capsule.height = 2f;
                if (capsule.radius < 0.25f) capsule.radius = 0.45f;
            });
        }

        private static void SetupBossPrefab(string path, Avatar avatar, AnimatorController controller, string animatorTarget)
        {
            EditPrefab(path, root => ConfigureAnimator(root, avatar, controller, animatorTarget));
        }

        private static void SetupEnemyPrefabs(Avatar avatar, AnimatorController controller)
        {
            string[] paths = Directory.GetFiles("Assets/Prefabs/Characters/Enemies", "*.prefab", SearchOption.TopDirectoryOnly)
                .Select(path => path.Replace('\\', '/')).OrderBy(path => path).ToArray();
            if (paths.Length != 9)
                Debug.LogWarning("[AnimationPipelineSetup] Expected 9 tourist prefabs, found " + paths.Length + ". Configuring all prefabs in the Enemies folder.");

            foreach (string path in paths)
            {
                EditPrefab(path, root =>
                {
                    root.tag = "Enemy";
                    ConfigureAnimator(root, avatar, controller, null);
                    CapsuleCollider capsule = AddIfMissing<CapsuleCollider>(root);
                    capsule.direction = 1;
                    if (capsule.height < 1.5f) capsule.height = 2f;
                    if (capsule.radius < 0.25f) capsule.radius = 0.4f;

                    Rigidbody body = AddIfMissing<Rigidbody>(root);
                    body.useGravity = false;
                    body.isKinematic = true;
                    body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

                    AddIfMissing<EnemyController>(root);
                    AddIfMissing<DamageFlash>(root);
                });
            }
        }

        private static void ConfigureAnimator(GameObject root, Avatar avatar, RuntimeAnimatorController controller, string targetName)
        {
            Transform target = FindTransform(root.transform, targetName);
            Animator animator = target != null ? target.GetComponent<Animator>() : null;
            if (animator == null) animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                GameObject owner = target != null ? target.gameObject : root;
                animator = owner.AddComponent<Animator>();
            }

            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(animator);
        }

        private static Transform FindTransform(Transform root, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindTransform(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static T AddIfMissing<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void EditPrefab(string path, Action<GameObject> edit)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                edit(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static CharacterPrefabRegistry CreateOrUpdateRegistry()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder(RegistryFolder);
            CharacterPrefabRegistry registry = AssetDatabase.LoadAssetAtPath<CharacterPrefabRegistry>(RegistryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<CharacterPrefabRegistry>();
                AssetDatabase.CreateAsset(registry, RegistryPath);
            }

            registry.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Player/Player.prefab");
            registry.enemyPrefabs = Directory.GetFiles("Assets/Prefabs/Characters/Enemies", "*.prefab", SearchOption.TopDirectoryOnly)
                .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/')))
                .Where(prefab => prefab != null).OrderBy(prefab => prefab.name).ToArray();
            registry.levelBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Bosses/TripleDelivery.prefab");
            registry.foodCartBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Bosses/FoodCart.prefab");
            registry.foodTruckBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Bosses/FoodTruck.prefab");
            registry.tripleDeliveryBossPrefab = registry.levelBossPrefab;
            EditorUtility.SetDirty(registry);
            return registry;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetupGameScenePlayer(GameObject playerPrefab)
        {
            if (playerPrefab == null || !File.Exists(GameScenePath)) return;
            string previousScenePath = SceneManager.GetActiveScene().path;
            Scene gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");
            if (oldPlayer != null && PrefabUtility.GetCorrespondingObjectFromSource(oldPlayer) == playerPrefab)
            {
                if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != GameScenePath)
                    EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
                return;
            }

            Vector3 position = oldPlayer != null ? oldPlayer.transform.position : new Vector3(0f, 1f, 0f);
            Quaternion rotation = oldPlayer != null ? oldPlayer.transform.rotation : Quaternion.identity;
            Vector3 scale = oldPlayer != null ? oldPlayer.transform.localScale : Vector3.one;
            PlayerController oldController = oldPlayer != null ? oldPlayer.GetComponent<PlayerController>() : null;
            Transform oldTransform = oldPlayer != null ? oldPlayer.transform : null;
            GameObject replacement = PrefabUtility.InstantiatePrefab(playerPrefab, gameScene) as GameObject;
            if (replacement == null) return;
            replacement.name = "Player";
            replacement.tag = "Player";
            replacement.transform.SetPositionAndRotation(position, rotation);
            replacement.transform.localScale = scale;

            PlayerController newController = replacement.GetComponent<PlayerController>();
            if (oldController != null && newController != null)
                EditorUtility.CopySerialized(oldController, newController);
            replacement.tag = "Player";
            if (newController != null) PrefabUtility.RecordPrefabInstancePropertyModifications(newController);

            foreach (CameraFollow cameraFollow in UnityEngine.Object.FindObjectsByType<CameraFollow>(FindObjectsSortMode.None))
                if ((oldTransform != null && cameraFollow.target == oldTransform) || cameraFollow.target == null)
                    cameraFollow.target = replacement.transform;
            foreach (WaveManager waveManager in UnityEngine.Object.FindObjectsByType<WaveManager>(FindObjectsSortMode.None))
                if ((oldTransform != null && waveManager.playerTransform == oldTransform) || waveManager.playerTransform == null)
                    waveManager.playerTransform = replacement.transform;

            if (oldPlayer != null)
            {
                Undo.DestroyObjectImmediate(oldPlayer);
            }

            EditorSceneManager.MarkSceneDirty(gameScene);
            EditorSceneManager.SaveScene(gameScene);
            if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != GameScenePath)
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
        }
    }
}

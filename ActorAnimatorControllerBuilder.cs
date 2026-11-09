using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace InfiniteAdventure.EditorTools
{
    /// <summary>Creates an AnimatorController wired to ActorAnimator's parameters. Drop your clips into the states afterwards.</summary>
    public static class ActorAnimatorControllerBuilder
    {
        [MenuItem("Game/Create Actor Animator Controller")]
        static void Create()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Animator Controller", "ActorAnimator", "controller", "Choose where to save it");
            if (string.IsNullOrEmpty(path)) return;

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("LightAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dash", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;
            var locomotion = sm.AddState("Locomotion"); // assign idle/run clip or a blend tree driven by Speed
            sm.defaultState = locomotion;

            AddAction(sm, locomotion, "LightAttack", "LightAttack");
            AddAction(sm, locomotion, "HeavyAttack", "HeavyAttack");
            AddAction(sm, locomotion, "Dash", "Dash");
            AddAction(sm, locomotion, "Hurt", "Hurt");

            var death = sm.AddState("Death");
            var toDeath = sm.AddAnyStateTransition(death);
            toDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
            toDeath.hasExitTime = false;
            toDeath.duration = 0.05f;
            toDeath.canTransitionToSelf = false;

            var revive = death.AddTransition(locomotion);
            revive.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDead");
            revive.hasExitTime = false;
            revive.duration = 0f;

            AssetDatabase.SaveAssets();
            Selection.activeObject = controller;
        }

        static void AddAction(AnimatorStateMachine sm, AnimatorState locomotion, string stateName, string trigger)
        {
            var state = sm.AddState(stateName);

            var enter = sm.AddAnyStateTransition(state);
            enter.AddCondition(AnimatorConditionMode.If, 0, trigger);
            enter.hasExitTime = false;
            enter.duration = 0.05f;
            enter.canTransitionToSelf = false;

            var back = state.AddTransition(locomotion);
            back.hasExitTime = true;
            back.exitTime = 0.9f;
            back.duration = 0.1f;
        }
    }
}

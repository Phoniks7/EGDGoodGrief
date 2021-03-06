using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Log is a simple task which will output the specified text and return success. It can be used for debugging.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=16")]
    [TaskIcon("{SkinColor}LogIcon.png")]
    public class Log : Action
    {
        // Text to output to the log.
        public string text = "";
        // Is this text an error?
        public bool logError = false;

        public override TaskStatus OnUpdate()
        {
            // Log the text and return success
            if (logError) {
                Debug.LogError(text);
            } else {
                Debug.Log(text);
            }
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            // Reset the properties back to their original values
            text = "";
            logError = false;
        }
    }
}
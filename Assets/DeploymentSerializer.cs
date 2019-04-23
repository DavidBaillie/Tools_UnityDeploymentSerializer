using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeploymentSerializer
{
    /// <summary>
    /// Class containing serialization methods for storing class data in and out of the engine.
    /// </summary>
    public static class DeploymentSerializer
    {
        private const string filenamePrefix = "DS_";
        private static string resourcesFilePath = UnityTools.getProjectFilePath() + "/Resources/";

        /// <summary>
        /// Saves the provided Object to a binary file. If marked as persistent the file is saved to the Resources folder to be rebuilt for 
        /// builds of the game.
        /// </summary>
        /// <typeparam name="T">Type of Object to save</typeparam>
        /// <param name="objectToSave">Object to be saved</param>
        /// <param name="isPersistentInBuild">Determines if the file is saved to the resources folder to keep persistent in game build</param>
        public static void saveObject<T>(T objectToSave, string fileName, bool isPersistentInBuild)
        {
            //Check if we're running in build or engine - Save to correct location
            if (UnityTools.isRunningInEngine())
            {
                makeSureResourcesExists();
                serializeObjectInEgnine(objectToSave, fileName);
            }
        }

        /// <summary>
        /// Saves the object to a binary file in the Resources project folder
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="objectToSave">Object to be serialized</param>
        /// <param name="fileName">Name to save file under</param>
        private static void serializeObjectInEgnine <T> (T objectToSave, string fileName)
        {
            string filePath = resourcesFilePath + filenamePrefix + fileName;
            Debug.Log(filePath);
        }

        /// <summary>
        /// Makes sure the Resources folder exists under the Assets folder. If it doesn't exist, create it.
        /// </summary>
        private static void makeSureResourcesExists ()
        {
            //If the folder doesn't exist, create it
            if (System.IO.Directory.Exists(resourcesFilePath) == false)
            {
                System.IO.Directory.CreateDirectory(resourcesFilePath);
            }
        }
    }

    /// <summary>
    /// Additional tools to help with extra functionality newer programmers may find useful. 
    /// </summary>
    public static class UnityTools
    {
        /// <summary>
        /// Is the game currently running in the Unity Engine?
        /// </summary>
        /// <returns>True if in engine, false if in build</returns>
        public static bool isRunningInEngine ()
        {
            return UnityEngine.Application.isEditor;
        }

        /// <summary>
        /// Returns the file path of the machine leading to the folder holding the game files. 
        /// When in engine -> Returns the 'projectFolder/Assets'
        /// When in build -> Returns the build folder
        /// </summary>
        /// <returns>String - File path leading to project folder</returns>
        public static string getProjectFilePath ()
        {
            return Application.dataPath;
        }

        /// <summary>
        /// Returns the file path to a persistent folder on the end machine typically used
        /// for saving and modifying data in builds of the game.
        /// </summary>
        /// <returns>String - persistent folder for saving/modifying data in builds</returns>
        public static string getPersistentFilePath ()
        {
            return Application.persistentDataPath;
        }
    }

    /// <summary>
    /// Class handles all logging related functions for when the asset needs to log a status message 
    /// with developers using the asset. 
    /// </summary>
    internal static class DS_MessageLogger
    {
        internal static void logMessageToUnityConsole (string message)
        {

        }
    }
}

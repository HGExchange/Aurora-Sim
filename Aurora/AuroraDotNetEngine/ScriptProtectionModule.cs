/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Mail;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using Aurora.Framework;

namespace Aurora.ScriptEngine.AuroraDotNetEngine
{
    public class ScriptProtectionModule
    {
        #region Declares

        private IConfig m_config;
        private ScriptEngine m_scriptEngine;
        private bool allowHTMLLinking = true;

        //Threat Level for scripts.
        private ThreatLevel m_MaxThreatLevel = 0;
        //List of all enabled APIs for scripts
        private List<string> EnabledAPIs = new List<string>();
        //Keeps track of whether the source has been compiled before
        public Dictionary<string, string> PreviouslyCompiled = new Dictionary<string, string> ();
        public ThreatLevelDefinition m_threatLevelNone;
        public ThreatLevelDefinition m_threatLevelNuisance;
        public ThreatLevelDefinition m_threatLevelVeryLow;
        public ThreatLevelDefinition m_threatLevelLow;
        public ThreatLevelDefinition m_threatLevelModerate;
        public ThreatLevelDefinition m_threatLevelHigh;
        public ThreatLevelDefinition m_threatLevelVeryHigh;
        public ThreatLevelDefinition m_threatLevelSevere;
        
        public Dictionary<UUID, UUID> ScriptsItems = new Dictionary<UUID, UUID>();
        public Dictionary<UUID, Dictionary<UUID, ScriptData>> Scripts = new Dictionary<UUID, Dictionary<UUID, ScriptData>>();
        
        public bool AllowHTMLLinking
        {
            get
            {
                return allowHTMLLinking;
            }
        }
        
        #endregion
        
        #region Constructor
        
        public ScriptProtectionModule(ScriptEngine engine, IConfig config)
		{
            m_config = config;
            m_scriptEngine = engine;
            EnabledAPIs = new List<string>(config.GetString("AllowedAPIs", "LSL").Split(','));

            allowHTMLLinking = config.GetBoolean("AllowHTMLLinking", true);

            m_threatLevelNone = new ThreatLevelDefinition (ThreatLevel.None, UserSetHelpers.ParseUserSetConfigSetting (config, "NoneUserSet", UserSet.None), this);
            m_threatLevelNuisance = new ThreatLevelDefinition (ThreatLevel.Nuisance, UserSetHelpers.ParseUserSetConfigSetting (config, "NuisanceUserSet", UserSet.None), this);
            m_threatLevelVeryLow = new ThreatLevelDefinition (ThreatLevel.VeryLow, UserSetHelpers.ParseUserSetConfigSetting (config, "VeryLowUserSet", UserSet.None), this);
            m_threatLevelLow = new ThreatLevelDefinition (ThreatLevel.Low, UserSetHelpers.ParseUserSetConfigSetting (config, "LowUserSet", UserSet.None), this);
            m_threatLevelModerate = new ThreatLevelDefinition (ThreatLevel.Moderate, UserSetHelpers.ParseUserSetConfigSetting (config, "ModerateUserSet", UserSet.None), this);
            m_threatLevelHigh = new ThreatLevelDefinition (ThreatLevel.High, UserSetHelpers.ParseUserSetConfigSetting (config, "HighUserSet", UserSet.None), this);
            m_threatLevelVeryHigh = new ThreatLevelDefinition (ThreatLevel.VeryHigh, UserSetHelpers.ParseUserSetConfigSetting (config, "VeryHighUserSet", UserSet.None), this);
            m_threatLevelSevere = new ThreatLevelDefinition (ThreatLevel.Severe, UserSetHelpers.ParseUserSetConfigSetting (config, "SevereUserSet", UserSet.None), this);
		}
        
		#endregion
        
        #region ThreatLevels

        public class ThreatLevelDefinition
        {
            private ThreatLevel m_threatLevel = ThreatLevel.None;
            private UserSet m_userSet = UserSet.None;
            private ScriptProtectionModule m_scriptProtectionModule = null;
            /// <summary>
            /// Which owners have access to which functions
            /// </summary>
            private Dictionary<string, List<UUID>> m_FunctionPerms = new Dictionary<string, List<UUID>> ();
            private List<UUID> m_allowedUsers = new List<UUID> ();
            private Dictionary<UUID, Dictionary<string, bool>> m_knownAllowedGroupFunctionsForAvatars = new Dictionary<UUID, Dictionary<string, bool>> ();
            private bool m_allowGroupPermissions = false;

            public ThreatLevelDefinition (ThreatLevel threatLevel, UserSet userSet, ScriptProtectionModule module)
            {
                m_threatLevel = threatLevel;
                m_userSet = userSet;
                m_scriptProtectionModule = module;
                m_allowGroupPermissions = m_scriptProtectionModule.m_config.GetBoolean ("AllowGroupThreatPermissionCheck", m_allowGroupPermissions);

                string perm = m_scriptProtectionModule.m_config.GetString ("Allow_" + m_threatLevel.ToString(), "");
                if (perm != "")
                {
                    string[] ids = perm.Split (',');
                    foreach (string id in ids)
                    {
                        string current = id.Trim ();
                        UUID uuid;

                        if (UUID.TryParse (current, out uuid))
                        {
                            if (uuid != UUID.Zero)
                                m_allowedUsers.Add (uuid);
                        }
                    }
                }
                perm = m_scriptProtectionModule.m_config.GetString ("Allow_All", "");
                if (perm != "")
                {
                    string[] ids = perm.Split (',');
                    foreach (string id in ids)
                    {
                        string current = id.Trim ();
                        UUID uuid;

                        if (UUID.TryParse (current, out uuid))
                        {
                            if (uuid != UUID.Zero)
                                m_allowedUsers.Add (uuid);
                        }
                    }
                }
            }

            public void CheckThreatLevel (string function, ISceneChildEntity m_host, string API)
            {
                if (CheckUser (m_host))
                    return;
                List<UUID> FunctionPerms = new List<UUID> ();
                if (!m_FunctionPerms.TryGetValue (function, out FunctionPerms))
                {
                    string perm = m_scriptProtectionModule.m_config.GetString ("Allow_" + function, "");
                    if (perm == "")
                    {
                        FunctionPerms = null; // a null value is default, which means check against the max threat level
                    }
                    else
                    {
                        bool allowed;

                        if (bool.TryParse (perm, out allowed))
                        {
                            // Boolean given
                            if (allowed)
                            {
                                FunctionPerms = new List<UUID> ();
                                FunctionPerms.Add (UUID.Zero);
                            }
                            else
                                FunctionPerms = new List<UUID> (); // Empty list = none
                        }
                        else
                        {
                            FunctionPerms = new List<UUID> ();

                            string[] ids = perm.Split (new char[] { ',' });
                            foreach (string id in ids)
                            {
                                string current = id.Trim ();
                                UUID uuid;

                                if (UUID.TryParse (current, out uuid))
                                {
                                    if (uuid != UUID.Zero)
                                        FunctionPerms.Add (uuid);
                                }
                            }
                        }
                        m_FunctionPerms[function] = FunctionPerms;
                    }
                }

                // If the list is null, then the value was true / undefined
                // Threat level governs permissions in this case
                //
                // If the list is non-null, then it is a list of UUIDs allowed
                // to use that particular function. False causes an empty
                // list and therefore means "no one"
                //
                // To allow use by anyone, the list contains UUID.Zero
                //
                if (FunctionPerms == null) // No list = true
                {
                    if (m_threatLevel > m_scriptProtectionModule.GetThreatLevel ().m_threatLevel)
                        m_scriptProtectionModule.Error ("Runtime Error: ",
                            String.Format (
                                "{0} permission denied.  Allowed threat level is {1} but function threat level is {2}.",
                                function, m_scriptProtectionModule.GetThreatLevel ().m_threatLevel, m_threatLevel));
                }
                else
                {
                    if (!FunctionPerms.Contains (UUID.Zero))
                    {
                        if (!FunctionPerms.Contains (m_host.OwnerID))
                        {
                            if (m_allowGroupPermissions)
                            {
                                Dictionary<string, bool> cachedFunctions;
                                //Check to see whether we have already evaluated this function for this user
                                if (m_knownAllowedGroupFunctionsForAvatars.TryGetValue (m_host.OwnerID, out cachedFunctions))
                                {
                                    if (cachedFunctions.ContainsKey (function))
                                    {
                                        if (cachedFunctions[function])
                                            return;
                                        else
                                            m_scriptProtectionModule.Error ("Runtime Error: ",
                                                String.Format ("{0} permission denied.  Prim owner is not in the list of users allowed to execute this function.",
                                                function));
                                    }
                                }
                                else
                                    cachedFunctions = new Dictionary<string, bool> ();
                                IGroupsModule groupsModule = m_host.ParentEntity.Scene.RequestModuleInterface<IGroupsModule> ();
                                if (groupsModule != null)
                                {
                                    bool success = false;
                                    foreach (UUID id in FunctionPerms)
                                    {
                                        if (groupsModule.GroupPermissionCheck (m_host.OwnerID, id, GroupPowers.None))
                                        {
                                            success = true;
                                            break;
                                        }
                                    }
                                    //Cache the success
                                    cachedFunctions[function] = success;
                                    if (!m_knownAllowedGroupFunctionsForAvatars.ContainsKey (m_host.OwnerID))
                                        m_knownAllowedGroupFunctionsForAvatars.Add (m_host.OwnerID, new Dictionary<string, bool> ());
                                    m_knownAllowedGroupFunctionsForAvatars[m_host.OwnerID] = cachedFunctions;

                                    if (success)
                                        return; //All is good
                                }
                            }
                            m_scriptProtectionModule.Error ("Runtime Error: ",
                                String.Format ("{0} permission denied.  Prim owner is not in the list of users allowed to execute this function.",
                                function));
                        }
                    }
                }
            }

            private bool CheckUser (ISceneChildEntity host)
            {
                if (m_allowedUsers.Contains (host.OwnerID))
                    return true;

                if (m_userSet == UserSet.ParcelOwners)
                {
                    IScenePresence av = host.ParentEntity.Scene.GetScenePresence (host.OwnerID);
                    ILandObject lo = null;
                    if (av != null)
                        lo = host.ParentEntity.Scene.RequestModuleInterface<IParcelManagementModule> ().GetLandObject (av.AbsolutePosition.X, av.AbsolutePosition.Y);
                    if (host.ParentEntity.Scene.Permissions.GenericParcelPermission (host.OwnerID, lo, 0))
                        return true;
                }
                else if ((m_userSet == UserSet.Administrators &&
                    host.ParentEntity.Scene.Permissions.IsGod (host.OwnerID)))
                {
                    m_allowedUsers.Add (host.OwnerID);//We don't need to lock as it blocks up above,
                    //and we don't need to Contains() either as we already let all users in above
                    return true;
                }
                return false;
            }

            public override string ToString ()
            {
                return string.Format ("ThreatLevel: {0}, UserSet : {1}", m_threatLevel.ToString (), m_userSet.ToString ());
            }
        }

        public ThreatLevelDefinition GetThreatLevel ()
		{
			if(m_MaxThreatLevel != 0)
				return GetDefinition(m_MaxThreatLevel);
            string risk = m_config.GetString("FunctionThreatLevel", "VeryLow");
			switch (risk)
			{
				case "None":
					m_MaxThreatLevel = ThreatLevel.None;
					break;
				case "VeryLow":
					m_MaxThreatLevel = ThreatLevel.VeryLow;
					break;
				case "Low":
					m_MaxThreatLevel = ThreatLevel.Low;
					break;
				case "Moderate":
					m_MaxThreatLevel = ThreatLevel.Moderate;
					break;
				case "High":
					m_MaxThreatLevel = ThreatLevel.High;
					break;
				case "VeryHigh":
					m_MaxThreatLevel = ThreatLevel.VeryHigh;
					break;
				case "Severe":
					m_MaxThreatLevel = ThreatLevel.Severe;
					break;
				default:
					break;
			}
            return GetDefinition(m_MaxThreatLevel);
		}

        public bool CheckAPI(string Name)
        {
            if (!EnabledAPIs.Contains(Name))
                return false;
            return true;
        }
		
		public void CheckThreatLevel(ThreatLevel level, string function, ISceneChildEntity m_host, string API)
        {
            GetDefinition(level).CheckThreatLevel (function, m_host, API);
        }

        public ThreatLevelDefinition GetDefinition (ThreatLevel level)
        {
            switch (level)
            {
                case ThreatLevel.None:
                    return m_threatLevelNone;
                case ThreatLevel.Nuisance:
                    return m_threatLevelNuisance;
                case ThreatLevel.VeryLow:
                    return m_threatLevelVeryLow;
                case ThreatLevel.Low:
                    return m_threatLevelLow;
                case ThreatLevel.Moderate:
                    return m_threatLevelModerate;
                case ThreatLevel.High:
                    return m_threatLevelHigh;
                case ThreatLevel.VeryHigh:
                    return m_threatLevelVeryHigh;
                case ThreatLevel.Severe:
                    return m_threatLevelSevere;
            }
            return null;
        }

		internal void Error(string surMessage, string msg)
        {
            throw new Exception(surMessage + msg);
        }

		#endregion
        
        #region Previously Compiled Scripts

        /// <summary>
        /// Reset all lists (if hard), if not hard, just reset previously compiled
        /// </summary>
        /// <param name="hard"></param>
        public void Reset(bool hard)
        {
            lock (PreviouslyCompiled)
            {
                PreviouslyCompiled.Clear();
            }
            if (hard)
            {
                lock (ScriptsItems)
                {
                    ScriptsItems.Clear();
                }
                lock (Scripts)
                {
                    Scripts.Clear();
                }
            }
        }

        public void AddPreviouslyCompiled (string source, ScriptData ID)
        {
            //string key = source.Length.ToString() + source.GetHashCode().ToString();
            string key = Util.Md5Hash (source);
            lock (PreviouslyCompiled)
            {
                if (!PreviouslyCompiled.ContainsKey (key))
                {
                    //PreviouslyCompiled.Add (source, ID.AssemblyName);
                    PreviouslyCompiled.Add (key, ID.AssemblyName);
                }
            }
        }

        public void RemovePreviouslyCompiled (string source)
        {
            //string key = source.Length.ToString() + source.GetHashCode().ToString();
            string key = Util.Md5Hash (source);
            lock (PreviouslyCompiled)
            {
                if (PreviouslyCompiled.ContainsKey (key))
                {
                    PreviouslyCompiled.Remove (key);
                    //PreviouslyCompiled.Remove (source);
                }
            }
        }

        public string TryGetPreviouslyCompiledScript (string source)
        {
            //string key = source.Length.ToString() + source.GetHashCode().ToString();
            string key = Util.Md5Hash (source);
            string assemblyName = "";
            PreviouslyCompiled.TryGetValue (key, out assemblyName);
            //PreviouslyCompiled.TryGetValue (source, out assemblyName);

            return assemblyName;
        }
        
        public ScriptData GetScript(UUID primID, UUID itemID)
        {
            Dictionary<UUID, ScriptData> Instances;
            lock (Scripts)
            {
                if (Scripts.TryGetValue(primID, out Instances))
                {
                    ScriptData ID = null;
                    Instances.TryGetValue(itemID, out ID);
                    return ID;
                }
            }
            return null;
        }
        
        public ScriptData GetScript(UUID itemID)
        {
            lock (ScriptsItems)
            {
                UUID primID;
                if (ScriptsItems.TryGetValue(itemID, out primID))
                    return GetScript(primID, itemID);
                return null;
            }
        }

        public ScriptData[] GetScripts(UUID primID)
        {
            Dictionary<UUID, ScriptData> Instances;
            lock (Scripts)
            {
                if (Scripts.TryGetValue(primID, out Instances))
                    return new List<ScriptData>(Instances.Values).ToArray();
            }
            return null;
        }
        
        public void AddNewScript(ScriptData ID)
        {
            lock (ScriptsItems)
            {
                if(ID.Part != null)
                    ScriptsItems[ID.ItemID] = ID.Part.UUID;
            }
            lock (Scripts)
            {
                Dictionary<UUID, ScriptData> Instances = new Dictionary<UUID, ScriptData>();
                if (!Scripts.TryGetValue(ID.Part.UUID, out Instances))
                    Instances = new Dictionary<UUID, ScriptData>();

                Instances[ID.ItemID] = ID;
                Scripts[ID.Part.UUID] = Instances;
            }
        }
        
        public ScriptData[] GetAllScripts()
        {
        	List<ScriptData> Ids = new List<ScriptData>();
            lock (Scripts)
            {
                foreach (Dictionary<UUID, ScriptData> Instances in Scripts.Values)
                {
                    foreach (ScriptData ID in Instances.Values)
                    {
                        Ids.Add(ID);
                    }
                }
            }
        	return Ids.ToArray();
        }
        
        public void RemoveScript(ScriptData Data)
        {
            lock (ScriptsItems)
            {
                ScriptsItems.Remove(Data.ItemID);
            }
            lock (Scripts)
            {
                Dictionary<UUID, ScriptData> Instances = new Dictionary<UUID, ScriptData>();
                if (Scripts.TryGetValue (Data.Part.UUID, out Instances))
                {
                    Instances.Remove (Data.ItemID);
                    if (Instances.Count > 0)
                        Scripts[Data.Part.UUID] = Instances;
                    else
                        Scripts.Remove (Data.Part.UUID);
                }
            }
        }
        
        #endregion
    }
}

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
using System.Collections.Generic;
using System.Text;
using Aurora.DataManager;
using Aurora.Framework;
using OpenMetaverse;
using Nini.Config;
using OpenSim.Framework;
using System.Xml;
using System.Xml.Serialization;
using OpenSim.Services.Interfaces;

namespace Aurora.Services.DataService
{
    public class LocalOfflineMessagesConnector : IOfflineMessagesConnector
	{
        private IGenericData GD = null;
        private int m_maxOfflineMessages = 20;

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore simBase, string defaultConnectionString)
        {
            GD = GenericData;

            if (source.Configs[Name] != null)
                defaultConnectionString = source.Configs[Name].GetString("ConnectionString", defaultConnectionString);

            GD.ConnectToDatabase(defaultConnectionString, "Generics", source.Configs["AuroraConnectors"].GetBoolean("ValidateTables", true));

            DataManager.DataManager.RegisterPlugin(Name+"Local", this);

            m_maxOfflineMessages = source.Configs["AuroraConnectors"].GetInt ("MaxOfflineMessages", m_maxOfflineMessages);
            if (source.Configs["AuroraConnectors"].GetString("OfflineMessagesConnector", "LocalConnector") == "LocalConnector")
            {
                DataManager.DataManager.RegisterPlugin(Name, this);
            }
        }

        public string Name
        {
            get { return "IOfflineMessagesConnector"; }
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Gets all offline messages for the user in GridInstantMessage format.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public GridInstantMessage[] GetOfflineMessages(UUID agentID)
		{
            //Get all the messages
            List<GridInstantMessage> Messages = GenericUtils.GetGenerics<GridInstantMessage>(agentID, "OfflineMessages", GD, new GridInstantMessage());
            //Clear them out now that we have them
            GenericUtils.RemoveGeneric(agentID, "OfflineMessages", GD);
            return Messages.ToArray();
		}

        /// <summary>
        /// Adds a new offline message for the user.
        /// </summary>
        /// <param name="message"></param>
        public bool AddOfflineMessage(GridInstantMessage message)
		{
            if(GenericUtils.GetGenericCount(message.toAgentID, "OfflineMessages", GD) < m_maxOfflineMessages)
            {
                GenericUtils.AddGeneric(message.toAgentID, "OfflineMessages", UUID.Random().ToString(), message.ToOSD(), GD);
                return true;
            }
            return false;
		}
	}
}

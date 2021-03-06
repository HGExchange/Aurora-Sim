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
using vector = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Vector3;
using rotation = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Quaternion;
using LSLInteger = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLInteger;
using LSLString = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using Aurora.ScriptEngine.AuroraDotNetEngine;
using Aurora.ScriptEngine.AuroraDotNetEngine.APIs.Interfaces;
using Aurora.ScriptEngine.AuroraDotNetEngine.CompilerTools;

namespace Aurora.ScriptEngine.AuroraDotNetEngine.Runtime
{
    public partial class ScriptBaseClass
    {
        // AA CONSTANTS
        public static readonly LSLString ENABLE_GRAVITY = "enable_gravity";
        public static readonly LSLString GRAVITY_FORCE_X = "gravity_force_x";
        public static readonly LSLString GRAVITY_FORCE_Y = "gravity_force_y";
        public static readonly LSLString GRAVITY_FORCE_Z = "gravity_force_z";
        public static readonly LSLString ADD_GRAVITY_POINT = "add_gravity_point";
        public static readonly LSLString ADD_GRAVITY_FORCE = "add_gravity_force";

        public static readonly LSLString START_TIME_REVERSAL_SAVING = "start_time_reversal_saving";
        public static readonly LSLString STOP_TIME_REVERSAL_SAVING = "stop_time_reversal_saving";
        public static readonly LSLString START_TIME_REVERSAL = "start_time_reversal";
        public static readonly LSLString STOP_TIME_REVERSAL = "stop_time_reversal";
    }
}

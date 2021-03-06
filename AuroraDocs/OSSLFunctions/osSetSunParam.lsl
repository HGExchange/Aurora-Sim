// ----------------------------------------------------------------
// Example / Sample Script to show function use.
//
// Script Title:    osSetSunParam.lsl
// Script Author:   WhiteStar Magic
// Threat Level:    None
// Script Source:   SUPPLEMENTAL http://opensimulator.org/wiki/osSetSunParam
//
// Notes: See Script Source reference for more detailed information
// This sample is full opensource and available to use as you see fit and desire.
// Threat Levels only apply to OSSL & AA Functions
// 
// ================================================================
// C# Source Line:      public void    osSetSunParam(string param, double value)
// Inworld Script Line: osSetSunParam(string param, float value); 
//
// Example of osSetSunParam
// Params  "year_length" = "number of days to a year"
// Params  "day_length" = "number of seconds to a day"
// Params  "day_night_offset" = "induces a horizon shift"
// Params  "update_interval" = "how often to update the sun's position in frames"
// Params  "day_time_sun_hour_scale" = "scales day light vs nite hours to change day/night ratio"
//
string Params = "day_length";
float Value = 4.0;
integer touched = FALSE;
default
{
    state_entry()
    {
        llSay(0,"Touch to see osSetSunParam work.");
    }
    touch_end(integer num)
    {
        if(touched)
        {
            touched = FALSE;
            Value = 4.0;
            osSetSunParam(Params, Value);
            llOwnerSay("Sun Params ("+Params+") = ["+(string)osSunGetParam(Params)+"]");
        }
        else
        {
            touched = TRUE;
            Value = 24.0;
            osSetSunParam(Params, Value);
            llOwnerSay("Sun Params ("+Params+") = ["+(string)osSunGetParam(Params)+"]");
        }
    }
}
﻿using STL_Showcase.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.Config
{
    public interface IUserSettings
    {
        string GetSettingString(UserSettingEnum setting);
        int GetSettingInt(UserSettingEnum setting);
        float GetSettingFloat(UserSettingEnum setting);
        void SetSettingString(UserSettingEnum setting, string val);
        void SetSettingInt(UserSettingEnum setting, int val);
        void SetSettingFloat(UserSettingEnum setting, float val);
    }
}

﻿using System;

namespace cefai
{
    public interface ICefaiGenerator
    {
        public void Main();
    }


    public class StratisAttribute : Attribute
    {
    }
    public class IoTDeviceAttribute : Attribute
    {
    }
}
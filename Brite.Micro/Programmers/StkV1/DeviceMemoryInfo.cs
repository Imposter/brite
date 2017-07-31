﻿namespace Brite.Micro.Programmers.StkV1
{
    public class DeviceMemoryInfo
    {
        public MemoryType MemoryType { get; private set; }
        public int Size { get; set; }
        public int PageSize { get; set; }

        public DeviceMemoryInfo(MemoryType type)
        {
            MemoryType = type;
        }
    }
}
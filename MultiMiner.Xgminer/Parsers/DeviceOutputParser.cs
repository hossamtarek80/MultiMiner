﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultiMiner.Xgminer.Parsers
{
    public static class DeviceOutputParser
    {
        public static void ParseTextForDevices(List<string> text, List<Device> devices)
        {
            ParseTextForGpuDevices(text, devices);
            ParseTextForUsbDevices(text, devices);
        }

        private static void ParseTextForUsbDevices(List<string> text, List<Device> devices)
        {
            bool inUsbList = false;

            string currentUsbManufacturer = string.Empty;
            string currentUsbProduct = string.Empty;

            foreach (string line in text)
            {
                if (Regex.Match(line, DevicePatterns.UsbDevicesFooter).Success)
                {
                    inUsbList = false;
                }

                if (inUsbList)
                {
                    Match match = Regex.Match(line, DevicePatterns.UsbManufacturer);
                    if (match.Success)
                        currentUsbManufacturer = match.Groups[1].Value.TrimEnd();

                    match = Regex.Match(line, DevicePatterns.UsbProduct);
                    if (match.Success)
                    {
                        currentUsbProduct = match.Groups[1].Value.TrimEnd();

                        Device device = new Device();
                        device.Platform = string.Empty;
                        device.Vendor = currentUsbManufacturer;
                        device.Version = string.Empty;
                        device.Name = currentUsbProduct;
                        device.Description = string.Empty;
                        device.Kind = DeviceKind.USB;

                        devices.Add(device);
                    }
                }

                if (Regex.Match(line, DevicePatterns.UsbDevicesHeader).Success)
                {
                    inUsbList = true;
                }
            }
        }

        private static void ParseTextForGpuDevices(List<string> text, List<Device> devices)
        {
            bool inPlatform = false;

            string currentPlatformVendor = string.Empty;
            string currentPlatformName = string.Empty;
            string currentPlatformVersion = string.Empty;
            string currentDeviceName = string.Empty;
            string currentDeviceDescription = string.Empty;

            List<string> names = new List<string>();
            List<string> descriptions = new List<string>();

            foreach (string line in text)
            {
                Match match = Regex.Match(line, DevicePatterns.PlatformVendor);
                if (match.Success)
                    currentPlatformVendor = match.Groups[1].Value.TrimEnd();

                match = Regex.Match(line, DevicePatterns.PlatformName);
                if (match.Success)
                    currentPlatformName = match.Groups[1].Value.TrimEnd();

                match = Regex.Match(line, DevicePatterns.PlatformVersion);
                if (match.Success)
                    currentPlatformVersion = match.Groups[1].Value.TrimEnd();
                
                if (Regex.Match(line, DevicePatterns.PlatformDevicesFooter).Success)
                {
                    for (int i = 0; i < names.Count; i++)
                    {
                        Device device = new Device();
                        device.Platform = currentPlatformName;
                        device.Vendor = currentPlatformVendor;
                        device.Version = currentPlatformVersion;

                        device.Name = names[i];

                        //may not be descriptions, e.g. under OS X
                        if (i < descriptions.Count)
                            device.Description = descriptions[i];

                        device.Kind = DeviceKind.GPU;

                        devices.Add(device);
                    }

                    inPlatform = false;
                    names.Clear();
                    descriptions.Clear();
                }

                if (inPlatform)
                {
                    match = Regex.Match(line, DevicePatterns.DeviceName);
                    if (match.Success)
                    {
                        currentDeviceName = match.Groups[1].Value.TrimEnd();
                        names.Add(currentDeviceName);
                    }

                    match = Regex.Match(line, DevicePatterns.DeviceDescription);
                    if (match.Success)
                    {
                        currentDeviceDescription = match.Groups[1].Value.TrimEnd();
                        descriptions.Add(currentDeviceDescription);
                    }

                }

                if (Regex.Match(line, DevicePatterns.PlatformDevicesHeader).Success)
                {
                    inPlatform = true;
                    names.Clear();
                    descriptions.Clear();
                }
            }
        }
    }
}

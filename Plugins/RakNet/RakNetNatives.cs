using System;
using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetNatives : NativeObjectSingleton<RakNetNatives>
    {
        [NativeMethod] public virtual bool PR_Init() => throw new NotImplementedException();
        [NativeMethod] public virtual int BS_New() => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_Delete(int bs) => throw new NotImplementedException();

        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Int(int bs, int type, int value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Float(int bs, int type, float value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Bool(int bs, int type, bool value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_String(int bs, int type, string value) => throw new NotImplementedException();

        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Int(int bs, int type, out int value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Float(int bs, int type, out float value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Bool(int bs, int type, out bool value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_String(int bs, int type, out string value, int size) => throw new NotImplementedException();

        [NativeMethod] public virtual bool PR_SendPacket(int bs, int playerid, int priority, int reliability) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_SendRPC(int bs, int playerid, int rpcid, int priority, int reliability) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_EmulateIncomingPacket(int bs, int playerid) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_EmulateIncomingRPC(int bs, int playerid, int rpcid) => throw new NotImplementedException();
    }
}
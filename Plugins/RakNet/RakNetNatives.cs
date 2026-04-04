using System;
using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetNatives : NativeObjectSingleton<RakNetNatives>
    {
        [NativeMethod] public virtual bool PR_Init() => throw new NotImplementedException();

        [NativeMethod] public virtual int BS_New() => throw new NotImplementedException();
        [NativeMethod] public virtual int BS_NewCopy(int bs) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_Delete(int bs) => throw new NotImplementedException();

        [NativeMethod] public virtual bool BS_Reset(int bs) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_ResetReadPointer(int bs) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_ResetWritePointer(int bs) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_IgnoreBits(int bs, int numberOfBits) => throw new NotImplementedException();

        [NativeMethod] public virtual bool BS_SetWriteOffset(int bs, int offset) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_GetWriteOffset(int bs, out int offset) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_SetReadOffset(int bs, int offset) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_GetReadOffset(int bs, out int offset) => throw new NotImplementedException();

        [NativeMethod] public virtual bool BS_GetNumberOfBitsUsed(int bs, out int number) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_GetNumberOfBytesUsed(int bs, out int number) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_GetNumberOfUnreadBits(int bs, out int number) => throw new NotImplementedException();
        [NativeMethod] public virtual bool BS_GetNumberOfBitsAllocated(int bs, out int number) => throw new NotImplementedException();

        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Int(int bs, int type, int value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Float(int bs, int type, float value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Bool(int bs, int type, bool value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_String(int bs, int type, string value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Float3(int bs, int type, float v1, float v2, float v3) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Float4(int bs, int type, float v1, float v2, float v3, float v4) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_WriteValue")] public virtual bool BS_WriteValue_Bits(int bs, int type, int value, int numberOfBits) => throw new NotImplementedException();

        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Int(int bs, int type, out int value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Float(int bs, int type, out float value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Bool(int bs, int type, out bool value) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_String(int bs, int type, out string value, int size) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Float3(int bs, int type, out float v1, out float v2, out float v3) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Float4(int bs, int type, out float v1, out float v2, out float v3, out float v4) => throw new NotImplementedException();
        [NativeMethod(Function = "BS_ReadValue")] public virtual bool BS_ReadValue_Bits(int bs, int type, out int value, int numberOfBits) => throw new NotImplementedException();

        [NativeMethod] public virtual bool PR_SendPacket(int bs, int playerid, int priority, int reliability, int orderingchannel = 0) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_SendRPC(int bs, int playerid, int rpcid, int priority, int reliability, int orderingchannel = 0) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_EmulateIncomingPacket(int bs, int playerid) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_EmulateIncomingRPC(int bs, int playerid, int rpcid) => throw new NotImplementedException();
        [NativeMethod] public virtual bool PR_RegHandler(int eventid, string publicname, int type) => throw new NotImplementedException();
    }
}
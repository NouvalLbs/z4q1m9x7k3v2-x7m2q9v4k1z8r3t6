using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.Memory
{
    public enum MemoryResult
    {
        OK,
        InvalidSize,
        InvalidPointer,
        InvalidIndex,
        InvalidIndexSize,
        OutOfMemory
    }

    public class MemoryNatives : NativeObjectSingleton<MemoryNatives>
    {
        // ── Managed Memory ────────────────────────────────────────────────────

        [NativeMethod]
        public virtual int MEM_new(int cells = 1)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_new_zero(int cells = 1)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_new_val(int value)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_new_arr(int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_clone(int pointer)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void MEM_delete(int pointer)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual bool MEM_is_valid_ptr(int pointer)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_get_size(int pointer)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_get_val(int pointer, int index = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod(3)]
        public virtual int MEM_get_arr(int pointer, int index, out int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_set_val(int pointer, int index, int value)
            => throw new NativeNotImplementedException();

        [NativeMethod(3)]
        public virtual int MEM_set_arr(int pointer, int index, int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_copy(int dest, int src, int size, int destIndex = 0, int srcIndex = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_zero(int pointer, int size, int index = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual MemoryResult MEM_get_last_result()
            => throw new NativeNotImplementedException();

        // ── Unmanaged Memory ──────────────────────────────────────────────────

        [NativeMethod]
        public virtual int MEM_UM_new(int cells = 1)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_new_zero(int cells = 1)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_new_val(int value)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_new_arr(int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_clone(int pointer, int index, int cells)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual void MEM_UM_delete(int pointer)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_get_val(int pointer, int index = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod(3)]
        public virtual int MEM_UM_get_arr(int pointer, int index, out int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_set_val(int pointer, int index, int value)
            => throw new NativeNotImplementedException();

        [NativeMethod(3)]
        public virtual int MEM_UM_set_arr(int pointer, int index, int[] arr, int arrSize)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_copy(int dest, int src, int size, int destIndex = 0, int srcIndex = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_zero(int pointer, int size, int index = 0)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_get_addr(ref int var)
            => throw new NativeNotImplementedException();

        [NativeMethod]
        public virtual int MEM_UM_get_amx_ptr()
            => throw new NativeNotImplementedException();
    }
}
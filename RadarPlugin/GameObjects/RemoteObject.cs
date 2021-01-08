using System;

namespace RadarPlugin.GameObjects {
    public class RemoteObject {
        public IntPtr Pointer { get; private set; }

        public virtual bool IsValid => Pointer != IntPtr.Zero;

        protected RemoteObject(IntPtr pointer) {
            Pointer = pointer;
        }

        public void UpdatePointer(IntPtr newPointer) {
            Pointer = newPointer;
            OnUpdatePointer(newPointer);
        }

        protected virtual void OnUpdatePointer(IntPtr newPointer) { }
    }
}
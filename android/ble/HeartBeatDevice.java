package cn.com.heaton.shiningmask.ble;

import android.util.Log;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleManager;

/* JADX INFO: loaded from: classes.dex */
public class HeartBeatDevice extends BleDevice {
    private DeviceListener deviceListener;

    public interface DeviceListener {
        void onDisconnected();
    }

    public HeartBeatDevice(BleManager<? extends BleDevice> bleManager, String str, String str2) {
        super(bleManager, str, str2);
    }

    public DeviceListener getDeviceListener() {
        return this.deviceListener;
    }

    public void setDeviceListener(DeviceListener deviceListener) {
        this.deviceListener = deviceListener;
    }

    @Override // com.cdbwsoft.library.ble.BleDevice
    protected void onDisconnected() {
        Log.e("HeartBeatDevice", "onDisconnected");
        super.onDisconnected();
        DeviceListener deviceListener = this.deviceListener;
        if (deviceListener != null) {
            deviceListener.onDisconnected();
        }
    }
}
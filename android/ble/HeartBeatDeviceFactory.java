package cn.com.heaton.shiningmask.ble;

import android.content.Context;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleDeviceFactory;
import com.cdbwsoft.library.ble.BleManager;

/* JADX INFO: loaded from: classes.dex */
public class HeartBeatDeviceFactory extends BleDeviceFactory<HeartBeatDevice> {
    @Override // com.cdbwsoft.library.ble.BleDeviceFactory
    public /* bridge */ /* synthetic */ BleDevice create(BleManager bleManager, String str, String str2) throws Exception {
        return create((BleManager<HeartBeatDevice>) bleManager, str, str2);
    }

    public HeartBeatDeviceFactory(Context context) {
        super(context);
    }

    @Override // com.cdbwsoft.library.ble.BleDeviceFactory
    public HeartBeatDevice create(BleManager<HeartBeatDevice> bleManager, String str, String str2) {
        return new HeartBeatDevice(bleManager, str, str2);
    }
}
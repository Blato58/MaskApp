package cn.com.heaton.shiningmask.model.data;

import com.cdbwsoft.library.ble.BleDevice;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class AppData {
    private List<BleDevice> deviceList = new ArrayList();

    public List<BleDevice> getDeviceList() {
        return this.deviceList;
    }
}
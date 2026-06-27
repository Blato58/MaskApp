package cn.com.heaton.shiningmask.model.data;

import android.graphics.Color;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.alibaba.fastjson2.JSONB;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleListener;
import com.cdbwsoft.library.ble.BleManager;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class TextAgreement {
    private static final String TAG = "TextAgreement";
    public static TextAgreement instance;
    BleListenerImpl bleListener;
    BleManager bleManager;
    Map<BleDevice, byte[]> dataMap = new HashMap();
    Map<BleDevice, TextData> dataMap1 = new HashMap();
    Map<BleDevice, TextAgreementListener> listenerMap = new HashMap();

    public interface TextAgreementListener {
        void onFinishSend(BleDevice bleDevice);
    }

    public static TextAgreement getInstance() {
        if (instance == null) {
            instance = new TextAgreement();
        }
        return instance;
    }

    private TextAgreement() {
        BleManager bleManager = ConnectActivity.getBleManager();
        this.bleManager = bleManager;
        if (bleManager != null) {
            BleListenerImpl bleListenerImpl = new BleListenerImpl();
            this.bleListener = bleListenerImpl;
            this.bleManager.registerBleListener(bleListenerImpl);
        }
    }

    public void sendTextTo1236(BleDevice bleDevice, byte[] bArr, int[] iArr, TextAgreementListener textAgreementListener) {
        if (bArr == null || iArr == null) {
            return;
        }
        LogUtil.d("=====发送 文本：文本长度：" + bArr.length + " 颜色长度：" + (iArr.length * 3));
        int length = bArr.length + (iArr.length * 3);
        byte[] bArr2 = new byte[length];
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < iArr.length; i++) {
            arrayList.add(Byte.valueOf((byte) Color.red(iArr[i])));
            arrayList.add(Byte.valueOf((byte) Color.green(iArr[i])));
            arrayList.add(Byte.valueOf((byte) Color.blue(iArr[i])));
        }
        int size = arrayList.size();
        byte[] bArr3 = new byte[size];
        for (int i2 = 0; i2 < arrayList.size(); i2++) {
            bArr3[i2] = ((Byte) arrayList.get(i2)).byteValue();
        }
        System.arraycopy(bArr, 0, bArr2, 0, bArr.length);
        System.arraycopy(bArr3, 0, bArr2, bArr.length, size);
        this.listenerMap.put(bleDevice, textAgreementListener);
        this.dataMap.put(bleDevice, bArr2);
        sendSendDataCommand(bleDevice, length, bArr.length);
    }

    private void sendSendDataCommand(BleDevice bleDevice, int i, int i2) {
        if (bleDevice == null) {
            return;
        }
        byte[] bArrInt2Bytes = Agreement.int2Bytes(i);
        byte[] bArrInt2Bytes2 = Agreement.int2Bytes(i2);
        byte[] encryptData = Agreement.getEncryptData(new byte[]{9, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 83, bArrInt2Bytes[0], bArrInt2Bytes[1], bArrInt2Bytes2[0], bArrInt2Bytes2[1], 0, 0, 0, 0, 0, 0, 0});
        LogUtil.d("========发送数据:" + Arrays.toString(encryptData) + "  result:" + bleDevice.writeCharacteristic(encryptData));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseSendDataCommand(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 68 && bArr[2] == 65 && bArr[3] == 84 && bArr[4] == 83 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public List<byte[]> getSendData(BleDevice bleDevice, byte[] bArr) {
        int length;
        if (bleDevice == null) {
            return null;
        }
        ArrayList arrayList = new ArrayList();
        LogUtil.d("MTU设置状态：" + bleDevice.isSetMtuStatus());
        int i = bleDevice.isSetMtuStatus() ? 98 : 18;
        if (bArr.length % i == 0) {
            length = bArr.length / i;
        } else {
            length = (bArr.length / i) + 1;
        }
        int i2 = 0;
        for (int i3 = 0; i3 < length; i3++) {
            if (i3 == length - 1) {
                int length2 = bArr.length - i2;
                byte[] bArr2 = new byte[i + 2];
                bArr2[0] = (byte) (length2 + 1);
                bArr2[1] = (byte) i3;
                System.arraycopy(bArr, i2, bArr2, 2, length2);
                arrayList.add(bArr2);
                i2 = 0;
            } else {
                byte[] bArr3 = new byte[i + 2];
                bArr3[0] = (byte) (i + 1);
                bArr3[1] = (byte) i3;
                System.arraycopy(bArr, i2, bArr3, 2, i);
                i2 += i;
                arrayList.add(bArr3);
            }
        }
        return arrayList;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendTextData(BleDevice bleDevice, byte[] bArr) {
        bleDevice.writeCharacteristicBy2(bArr);
        LogUtil.d("数据发送:" + bleDevice.getBleName() + " frameData:" + ByteUtils.binaryToHexString(bArr));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendSendDataFinishCommand(BleDevice bleDevice) {
        if (bleDevice == null) {
            return;
        }
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{5, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 67, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseDataCheck(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && bArr[1] == 68 && bArr[2] == 65 && bArr[3] == 84 && bArr[4] == 67 && bArr[5] == 80 && bArr[6] == 79 && bArr[7] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseErrorDataCheck(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && ((char) bArr[1]) == 'E' && ((char) bArr[2]) == 'R' && ((char) bArr[3]) == 'R' && ((char) bArr[4]) == 'O' && ((char) bArr[5]) == 'R';
    }

    class BleListenerImpl extends BleListener {
        BleListenerImpl() {
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onChanged(BleDevice bleDevice, byte[] bArr) {
            LogUtil.d("下位机发数据给上位机:" + bleDevice.getBleName());
            if (TextAgreement.this.dataMap.containsKey(bleDevice) && TextAgreement.this.listenerMap.containsKey(bleDevice)) {
                byte[] bArr2 = new byte[bArr.length];
                System.arraycopy(bArr, 0, bArr2, 0, bArr.length);
                byte[] decodeData = Agreement.getDecodeData(bArr2);
                StringBuffer stringBuffer = new StringBuffer();
                for (byte b : decodeData) {
                    stringBuffer.append((char) b);
                }
                LogUtil.d("MCU回复App:" + bleDevice.getBleName() + " " + stringBuffer.toString());
                if (TextAgreement.this.parseSendDataCommand(decodeData)) {
                    TextData textData = new TextData(0, TextAgreement.this.getSendData(bleDevice, TextAgreement.this.dataMap.get(bleDevice)));
                    TextAgreement.this.dataMap1.put(bleDevice, textData);
                    LogUtil.d("设备数：" + TextAgreement.this.dataMap1.size());
                    TextAgreement.this.sendTextData(bleDevice, textData.getDataList().get(textData.getCurIndex()));
                } else if (TextAgreement.this.isResultReOk(decodeData)) {
                    TextData textData2 = TextAgreement.this.dataMap1.get(bleDevice);
                    if (textData2 == null) {
                        return;
                    }
                    List<byte[]> dataList = textData2.getDataList();
                    int curIndex = textData2.getCurIndex();
                    if (curIndex < dataList.size() - 1) {
                        int i = curIndex + 1;
                        textData2.setCurIndex(i);
                        TextAgreement.this.sendTextData(bleDevice, dataList.get(i));
                    } else if (curIndex == dataList.size() - 1) {
                        LogUtil.d("数据发送完成");
                        TextAgreement.this.sendSendDataFinishCommand(bleDevice);
                    }
                } else if (TextAgreement.this.parseDataCheck(decodeData)) {
                    LogUtil.d("返回DATCPOK");
                    TextAgreementListener textAgreementListener = TextAgreement.this.listenerMap.get(bleDevice);
                    if (textAgreementListener != null) {
                        textAgreementListener.onFinishSend(bleDevice);
                    }
                    TextAgreement.this.dataMap.remove(bleDevice);
                    TextAgreement.this.dataMap1.remove(bleDevice);
                    TextAgreement.this.listenerMap.remove(bleDevice);
                } else if (TextAgreement.this.parseErrorDataCheck(decodeData)) {
                    LogUtil.d("返回ERROR=");
                    TextAgreementListener textAgreementListener2 = TextAgreement.this.listenerMap.get(bleDevice);
                    if (textAgreementListener2 != null) {
                        textAgreementListener2.onFinishSend(bleDevice);
                    }
                    TextAgreement.this.dataMap.remove(bleDevice);
                    TextAgreement.this.dataMap1.remove(bleDevice);
                    TextAgreement.this.listenerMap.remove(bleDevice);
                }
                super.onChanged(bleDevice, bArr);
            }
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onConnectionChanged(BleDevice bleDevice) {
            super.onConnectionChanged(bleDevice);
            if (bleDevice != null && bleDevice.isDisConnect() && TextAgreement.this.dataMap.containsKey(bleDevice) && TextAgreement.this.listenerMap.containsKey(bleDevice)) {
                LogUtil.e("设备断开连接：" + bleDevice);
                TextAgreementListener textAgreementListener = TextAgreement.this.listenerMap.get(bleDevice);
                if (textAgreementListener != null) {
                    textAgreementListener.onFinishSend(bleDevice);
                }
                TextAgreement.this.dataMap.remove(bleDevice);
                TextAgreement.this.dataMap1.remove(bleDevice);
                TextAgreement.this.listenerMap.remove(bleDevice);
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isResultReOk(byte[] bArr) {
        return bArr != null && bArr.length >= 5 && bArr[1] == 82 && bArr[2] == 69 && bArr[3] == 79 && bArr[4] == 75;
    }

    public void clear() {
        BleManager bleManager = this.bleManager;
        if (bleManager != null) {
            bleManager.unRegisterBleListener(this.bleListener);
            instance = null;
        }
    }
}
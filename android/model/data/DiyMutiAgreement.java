package cn.com.heaton.shiningmask.model.data;

import android.graphics.Color;
import android.os.Handler;
import cn.com.heaton.shiningmask.model.bean.DiyData;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.alibaba.fastjson2.JSONB;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleListener;
import com.cdbwsoft.library.ble.BleManager;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import kotlin.jvm.internal.ByteCompanionObject;

/* JADX INFO: loaded from: classes.dex */
public class DiyMutiAgreement {
    public static DiyMutiAgreement instance;
    BleListenerImpl bleListener;
    BleManager bleManager;
    int curSendDiyImageIndex;
    Map<BleDevice, byte[]> dataMap = new HashMap();
    Map<BleDevice, TextData> dataMap1 = new HashMap();
    Map<BleDevice, SendDiyData> sendDataMap = new HashMap();
    Map<BleDevice, DiyAgreementListener> listenerMap = new HashMap();
    List<DiyImageData> sendDataList = new ArrayList();
    Handler handler = new Handler();
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.model.data.DiyMutiAgreement.1
        @Override // java.lang.Runnable
        public void run() {
        }
    };

    public interface DiyAgreementListener {
        void onFinishSend(BleDevice bleDevice);

        void onManyOk(BleDevice bleDevice);

        void onStartSendCommand(BleDevice bleDevice);
    }

    public static DiyMutiAgreement getInstance() {
        if (instance == null) {
            instance = new DiyMutiAgreement();
        }
        return instance;
    }

    private DiyMutiAgreement() {
        BleManager bleManager = ConnectActivity.getBleManager();
        this.bleManager = bleManager;
        if (bleManager != null) {
            BleListenerImpl bleListenerImpl = new BleListenerImpl();
            this.bleListener = bleListenerImpl;
            this.bleManager.registerBleListener(bleListenerImpl);
        }
    }

    public void sendMutiImage(BleDevice bleDevice, List<DiyData> list, DiyAgreementListener diyAgreementListener) {
        if (list != null) {
            sendMutiImageCommand(bleDevice, list.size(), diyAgreementListener);
        }
    }

    public void sendultiDiy(BleDevice bleDevice, List<DiyData> list) {
        this.curSendDiyImageIndex = 0;
        this.sendDataList.clear();
        if (list != null) {
            for (int i = 0; i < list.size(); i++) {
                DiyData diyData = list.get(i);
                byte[] diyBytes1236 = getDiyBytes1236(diyData.getData());
                ArrayList<Integer> colorArray = diyData.getColorArray();
                if (colorArray != null) {
                    LogUtil.d("=====发送 文本：文本长度：" + diyBytes1236.length + " 颜色长度：" + colorArray.size());
                    byte[] bArr = new byte[diyBytes1236.length + (colorArray.size() * 3)];
                    byte[] colorArray2 = getColorArray(colorArray);
                    System.arraycopy(diyBytes1236, 0, bArr, 0, diyBytes1236.length);
                    System.arraycopy(colorArray2, 0, bArr, diyBytes1236.length, colorArray2.length);
                    DiyImageData diyImageData = new DiyImageData();
                    diyImageData.setCurIndex(0);
                    diyImageData.setDataLength(diyBytes1236.length);
                    diyImageData.setSendData(bArr);
                    this.sendDataList.add(diyImageData);
                }
            }
            if (this.sendDataList.isEmpty()) {
                return;
            }
            this.sendDataMap.put(bleDevice, new SendDiyData(0, this.sendDataList));
            DiyImageData diyImageData2 = this.sendDataList.get(0);
            sendDiyImageData(diyImageData2.getSendData(), diyImageData2.getDataLength(), bleDevice, 0);
        }
    }

    private byte[] getColorArray(List<Integer> list) {
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < list.size(); i++) {
            arrayList.add(Byte.valueOf((byte) Color.red(list.get(i).intValue())));
            arrayList.add(Byte.valueOf((byte) Color.green(list.get(i).intValue())));
            arrayList.add(Byte.valueOf((byte) Color.blue(list.get(i).intValue())));
        }
        byte[] bArr = new byte[arrayList.size()];
        for (int i2 = 0; i2 < arrayList.size(); i2++) {
            bArr[i2] = ((Byte) arrayList.get(i2)).byteValue();
        }
        return bArr;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendDiyImageData(byte[] bArr, int i, BleDevice bleDevice, int i2) {
        if (bArr != null) {
            this.dataMap.put(bleDevice, bArr);
            sendSendDataCommand(bleDevice, bArr.length, i);
        }
    }

    private void sendMutiImageCommand(BleDevice bleDevice, int i, DiyAgreementListener diyAgreementListener) {
        if (bleDevice == null) {
            return;
        }
        this.listenerMap.put(bleDevice, diyAgreementListener);
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{6, JSONB.Constants.BC_STR_ASCII_FIX_4, 65, JSONB.Constants.BC_STR_ASCII_FIX_5, 89, (byte) i, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendMutiImageOkCommand(BleDevice bleDevice) {
        if (bleDevice == null) {
            return;
        }
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{7, JSONB.Constants.BC_STR_ASCII_FIX_4, 65, JSONB.Constants.BC_STR_ASCII_FIX_5, 67, 80, 79, 75, 0, 0, 0, 0, 0, 0, 0, 0}));
    }

    private void sendSendDataCommand(BleDevice bleDevice, int i, int i2) {
        if (bleDevice == null) {
            return;
        }
        byte[] bArrInt2Bytes = Agreement.int2Bytes(i);
        byte[] bArrInt2Bytes2 = Agreement.int2Bytes(i2);
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{9, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 83, bArrInt2Bytes[0], bArrInt2Bytes[1], bArrInt2Bytes2[0], bArrInt2Bytes2[1], 1, 0, 0, 0, 0, 0, 0}));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseSendDataCommand(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 68 && bArr[2] == 65 && bArr[3] == 84 && bArr[4] == 83 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isResultReOk(byte[] bArr) {
        return bArr != null && bArr.length >= 5 && bArr[1] == 82 && bArr[2] == 69 && bArr[3] == 79 && bArr[4] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public List<byte[]> getSendData(BleDevice bleDevice, byte[] bArr) {
        int length;
        ArrayList arrayList = new ArrayList();
        if (bleDevice == null) {
            return null;
        }
        arrayList.clear();
        LogUtil.d("MTU设置状态：" + bleDevice.isSetMtuStatus());
        int i = bleDevice.isSetMtuStatus() ? 98 : 18;
        LogUtil.d("数据总长度：" + bArr.length);
        if (bArr.length % i == 0) {
            length = bArr.length / i;
        } else {
            length = (bArr.length / i) + 1;
        }
        LogUtil.d("frameTotal：" + length);
        int i2 = 0;
        for (int i3 = 0; i3 < length; i3++) {
            if (i3 == length - 1) {
                int length2 = bArr.length - i2;
                byte[] bArr2 = new byte[i + 2];
                bArr2[0] = (byte) (length2 + 1);
                bArr2[1] = (byte) i3;
                System.arraycopy(bArr, i2, bArr2, 2, length2);
                LogUtil.d("最后的数据：" + length2 + " index:0");
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
    public void sendSendDataFinishCommand(BleDevice bleDevice) {
        LogUtil.d("发送数据完成命令");
        if (bleDevice == null) {
            return;
        }
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{5, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 67, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseDataCheck(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && bArr[1] == 68 && bArr[2] == 65 && bArr[3] == 84 && bArr[4] == 67 && bArr[5] == 80 && bArr[6] == 79 && bArr[7] == 75;
    }

    /* JADX WARN: Failed to find 'out' block for switch in B:10:0x0018. Please report as an issue. */
    private byte[] getDiyBytes1236(byte[] bArr) {
        int i;
        int i2;
        if (bArr == null) {
            return null;
        }
        byte[] bArr2 = new byte[72];
        byte b = 0;
        byte b2 = 0;
        for (int i3 = 0; i3 < bArr.length; i3++) {
            int i4 = i3 / 12;
            int i5 = i3 % 12;
            if (bArr[i3] == 1) {
                switch (i5) {
                    case 0:
                        i = b | ByteCompanionObject.MIN_VALUE;
                        b = (byte) i;
                        break;
                    case 1:
                        i = b | JSONB.Constants.BC_INT32_SHORT_MIN;
                        b = (byte) i;
                        break;
                    case 2:
                        i = b | 32;
                        b = (byte) i;
                        break;
                    case 3:
                        i = b | JSONB.Constants.BC_INT32_NUM_16;
                        b = (byte) i;
                        break;
                    case 4:
                        i = b | 8;
                        b = (byte) i;
                        break;
                    case 5:
                        i = b | 4;
                        b = (byte) i;
                        break;
                    case 6:
                        i = b | 2;
                        b = (byte) i;
                        break;
                    case 7:
                        i = b | 1;
                        b = (byte) i;
                        break;
                    case 8:
                        i2 = b2 | ByteCompanionObject.MIN_VALUE;
                        b2 = (byte) i2;
                        break;
                    case 9:
                        i2 = b2 | JSONB.Constants.BC_INT32_SHORT_MIN;
                        b2 = (byte) i2;
                        break;
                    case 10:
                        i2 = b2 | 32;
                        b2 = (byte) i2;
                        break;
                    case 11:
                        i2 = b2 | JSONB.Constants.BC_INT32_NUM_16;
                        b2 = (byte) i2;
                        break;
                }
            }
            if (i5 == 11) {
                int i6 = i4 * 2;
                bArr2[i6] = b;
                bArr2[i6 + 1] = b2;
                b = 0;
                b2 = 0;
            }
        }
        return bArr2;
    }

    class BleListenerImpl extends BleListener {
        BleListenerImpl() {
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onChanged(BleDevice bleDevice, byte[] bArr) {
            LogUtil.d("收到硬件回的数据：" + ByteUtils.binaryToHexString(bArr));
            if (DiyMutiAgreement.this.listenerMap.containsKey(bleDevice)) {
                System.arraycopy(bArr, 0, new byte[bArr.length], 0, bArr.length);
                byte[] decodeData = Agreement.getDecodeData(bArr);
                StringBuffer stringBuffer = new StringBuffer();
                for (byte b : decodeData) {
                    stringBuffer.append((char) b);
                }
                LogUtil.d("MCU回复App:" + bleDevice.getBleName() + " " + stringBuffer.toString());
                if (DiyMutiAgreement.this.parseSendDataCommand(decodeData)) {
                    TextData textData = new TextData(0, DiyMutiAgreement.this.getSendData(bleDevice, DiyMutiAgreement.this.dataMap.get(bleDevice)));
                    DiyMutiAgreement.this.dataMap1.put(bleDevice, textData);
                    LogUtil.d("设备数：" + DiyMutiAgreement.this.dataMap1.size());
                    DiyMutiAgreement.this.setStartSendListener(bleDevice);
                    DiyMutiAgreement.this.sendTextData(bleDevice, textData.getDataList().get(textData.getCurIndex()));
                    return;
                }
                if (DiyMutiAgreement.this.isResultReOk(decodeData)) {
                    LogUtil.d("硬件回：REOK");
                    DiyMutiAgreement.this.setStartSendListener(bleDevice);
                    TextData textData2 = DiyMutiAgreement.this.dataMap1.get(bleDevice);
                    if (textData2 == null) {
                        return;
                    }
                    List<byte[]> dataList = textData2.getDataList();
                    int curIndex = textData2.getCurIndex();
                    if (curIndex < dataList.size() - 1) {
                        int i = curIndex + 1;
                        textData2.setCurIndex(i);
                        DiyMutiAgreement.this.sendTextData(bleDevice, dataList.get(i));
                        return;
                    } else {
                        if (curIndex == dataList.size() - 1) {
                            DiyMutiAgreement.this.sendSendDataFinishCommand(bleDevice);
                            return;
                        }
                        return;
                    }
                }
                if (DiyMutiAgreement.this.parseDataCheck(decodeData)) {
                    DiyMutiAgreement.this.setStartSendListener(bleDevice);
                    LogUtil.d("返回DATCPOK,发送MANCP");
                    SendDiyData sendDiyData = DiyMutiAgreement.this.sendDataMap.get(bleDevice);
                    if (sendDiyData == null) {
                        return;
                    }
                    List<DiyImageData> dataList2 = sendDiyData.getDataList();
                    int curIndex2 = sendDiyData.getCurIndex();
                    if (curIndex2 < dataList2.size() - 1) {
                        LogUtil.d("数据发送完成,发下一张图片");
                        int i2 = curIndex2 + 1;
                        sendDiyData.setCurIndex(i2);
                        DiyImageData diyImageData = dataList2.get(i2);
                        DiyMutiAgreement.this.sendDiyImageData(diyImageData.getSendData(), diyImageData.getDataLength(), bleDevice, 1);
                        return;
                    }
                    if (curIndex2 == dataList2.size() - 1) {
                        LogUtil.d("所有图片数据发送完成");
                        DiyMutiAgreement.this.sendMutiImageOkCommand(bleDevice);
                        return;
                    }
                    return;
                }
                if (DiyMutiAgreement.this.parseErrorDataCheck(decodeData)) {
                    LogUtil.d("返回ERROR=");
                    DiyAgreementListener diyAgreementListener = DiyMutiAgreement.this.listenerMap.get(bleDevice);
                    if (diyAgreementListener != null) {
                        diyAgreementListener.onFinishSend(bleDevice);
                    }
                    DiyMutiAgreement.this.dataMap.remove(bleDevice);
                    DiyMutiAgreement.this.dataMap1.remove(bleDevice);
                    DiyMutiAgreement.this.sendDataMap.remove(bleDevice);
                    DiyMutiAgreement.this.listenerMap.remove(bleDevice);
                    return;
                }
                if (DiyMutiAgreement.this.parseSendMutiDataOkCheck(decodeData)) {
                    LogUtil.d("返回MANCPOK=");
                    DiyAgreementListener diyAgreementListener2 = DiyMutiAgreement.this.listenerMap.get(bleDevice);
                    if (diyAgreementListener2 != null) {
                        diyAgreementListener2.onFinishSend(bleDevice);
                    }
                    DiyMutiAgreement.this.dataMap.remove(bleDevice);
                    DiyMutiAgreement.this.dataMap1.remove(bleDevice);
                    DiyMutiAgreement.this.sendDataMap.remove(bleDevice);
                    DiyMutiAgreement.this.listenerMap.remove(bleDevice);
                    return;
                }
                if (DiyMutiAgreement.this.parseSendMutiDataCheck(decodeData)) {
                    LogUtil.d("MANY OK=");
                    DiyAgreementListener diyAgreementListener3 = DiyMutiAgreement.this.listenerMap.get(bleDevice);
                    if (diyAgreementListener3 != null) {
                        diyAgreementListener3.onManyOk(bleDevice);
                    }
                }
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseErrorDataCheck(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && ((char) bArr[1]) == 'E' && ((char) bArr[2]) == 'R' && ((char) bArr[3]) == 'R' && ((char) bArr[4]) == 'O' && ((char) bArr[5]) == 'R';
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setStartSendListener(BleDevice bleDevice) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onStartSendCommand(bleDevice);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseSendMutiDataCheck(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && ((char) bArr[1]) == 'M' && ((char) bArr[2]) == 'A' && ((char) bArr[3]) == 'N' && ((char) bArr[4]) == 'Y' && ((char) bArr[5]) == 'O' && ((char) bArr[6]) == 'K';
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean parseSendMutiDataOkCheck(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && ((char) bArr[1]) == 'M' && ((char) bArr[2]) == 'A' && ((char) bArr[3]) == 'N' && ((char) bArr[4]) == 'C' && ((char) bArr[5]) == 'P' && ((char) bArr[6]) == 'O';
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendTextData(BleDevice bleDevice, byte[] bArr) {
        bleDevice.writeCharacteristicBy2(bArr);
        LogUtil.d("数据发送:" + bleDevice.getBleName() + " frameData:" + ByteUtils.binaryToHexString(bArr));
    }

    public void clear() {
        BleManager bleManager = this.bleManager;
        if (bleManager != null) {
            bleManager.unRegisterBleListener(this.bleListener);
            Map<BleDevice, DiyAgreementListener> map = this.listenerMap;
            if (map != null) {
                map.clear();
            }
            Map<BleDevice, byte[]> map2 = this.dataMap;
            if (map2 != null) {
                map2.clear();
            }
            Map<BleDevice, SendDiyData> map3 = this.sendDataMap;
            if (map3 != null) {
                map3.clear();
            }
            instance = null;
        }
    }
}
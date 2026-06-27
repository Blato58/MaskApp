package cn.com.heaton.shiningmask.model.data;

import cn.com.heaton.shiningmask.dao.bean.CropImage;
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

/* JADX INFO: loaded from: classes.dex */
public class DiyAgreement {
    public static DiyAgreement instance;
    BleListenerImpl bleListener;
    BleManager bleManager;
    private CropImage cropImage;
    Map<BleDevice, byte[]> dataMap = new HashMap();
    Map<BleDevice, TextData> dataMap1 = new HashMap();
    Map<BleDevice, DiyAgreementListener> listenerMap = new HashMap();

    public static abstract class DiyAgreementListener {
        public void onDeleteDiyImageOk(BleDevice bleDevice) {
        }

        public void onFaceOk(BleDevice bleDevice) {
        }

        public void onFinishSend(BleDevice bleDevice) {
        }

        public void onGetDiyImageCountOk(int i) {
        }

        public void onIsSynDiyImage(boolean z) {
        }

        public void onPlayDiyImageOk(BleDevice bleDevice) {
        }
    }

    public static DiyAgreement getInstance() {
        if (instance == null) {
            instance = new DiyAgreement();
        }
        return instance;
    }

    private DiyAgreement() {
        BleManager bleManager = ConnectActivity.getBleManager();
        this.bleManager = bleManager;
        if (bleManager != null) {
            BleListenerImpl bleListenerImpl = new BleListenerImpl();
            this.bleListener = bleListenerImpl;
            this.bleManager.registerBleListener(bleListenerImpl);
        }
    }

    public void sendDiy(BleDevice bleDevice, CropImage cropImage, DiyAgreementListener diyAgreementListener) {
        if (cropImage != null) {
            this.cropImage = cropImage;
            byte[] imageData = cropImage.getImageData();
            if (imageData != null) {
                LogUtil.d("发送的diy数据：" + imageData.length + " imageIndex:" + cropImage.getImageIndex() + " imageData:");
                this.dataMap.put(bleDevice, imageData);
                this.listenerMap.put(bleDevice, diyAgreementListener);
                sendSendDataCommand(bleDevice, imageData.length, cropImage.getImageIndex());
            }
        }
    }

    public void playDiyImage(BleDevice bleDevice, List<CropImage> list, DiyAgreementListener diyAgreementListener, int i) {
        byte[] playDiyImage2;
        if (list != null) {
            byte[] bArr = new byte[list.size()];
            for (int i2 = 0; i2 < list.size(); i2++) {
                bArr[i2] = (byte) list.get(i2).getImageIndex();
            }
            if (i == 0) {
                playDiyImage2 = Agreement.getPlayDiyImage(list.size(), bArr);
            } else {
                playDiyImage2 = Agreement.getPlayDiyImage2(bArr);
            }
            if (playDiyImage2 != null) {
                LogUtil.d("播放diy图片数据：" + ByteUtils.binaryToHexString(playDiyImage2));
                this.listenerMap.put(bleDevice, diyAgreementListener);
                bleDevice.writeCharacteristic(Agreement.getEncryptData(playDiyImage2));
            }
        }
    }

    public void deleteDiyImage(BleDevice bleDevice, List<CropImage> list, DiyAgreementListener diyAgreementListener, int i) {
        byte[] deleteDiyImage2;
        if (list != null) {
            byte[] bArr = new byte[list.size()];
            for (int i2 = 0; i2 < list.size(); i2++) {
                LogUtil.d("删除Diy图片：" + list.get(i2).getImageIndex() + "  type:" + i);
                bArr[i2] = (byte) list.get(i2).getImageIndex();
            }
            if (i == 0) {
                deleteDiyImage2 = Agreement.getDeleteDiyImage(list.size(), bArr);
            } else {
                deleteDiyImage2 = Agreement.getDeleteDiyImage2(bArr);
            }
            if (deleteDiyImage2 != null) {
                LogUtil.d("删除diy图片数据：" + ByteUtils.binaryToHexString(deleteDiyImage2));
                this.listenerMap.put(bleDevice, diyAgreementListener);
                bleDevice.writeCharacteristic(Agreement.getEncryptData(deleteDiyImage2));
            }
        }
    }

    public void getDeviceImageNum(BleDevice bleDevice, DiyAgreementListener diyAgreementListener) {
        byte[] diyImageCount = Agreement.getDiyImageCount();
        if (diyImageCount != null) {
            LogUtil.d("获取设备上的图片数量：" + ByteUtils.binaryToHexString(diyImageCount));
            this.listenerMap.put(bleDevice, diyAgreementListener);
            bleDevice.writeCharacteristic(Agreement.getEncryptData(diyImageCount));
        }
    }

    public void isSynImage(BleDevice bleDevice, int i, int i2, DiyAgreementListener diyAgreementListener) {
        byte[] diyImageTime = Agreement.getDiyImageTime((byte) i, i2);
        if (diyImageTime != null) {
            LogUtil.d("是否同步图片：" + ByteUtils.binaryToHexString(diyImageTime));
            this.listenerMap.put(bleDevice, diyAgreementListener);
            bleDevice.writeCharacteristic(Agreement.getEncryptData(diyImageTime));
        }
    }

    public void sendDiyImageSetting(BleDevice bleDevice, byte[] bArr, DiyAgreementListener diyAgreementListener) {
        if (bArr != null) {
            this.listenerMap.put(bleDevice, diyAgreementListener);
            LogUtil.d("发送变脸设置：" + ByteUtils.binaryToHexString(bArr));
            bleDevice.writeCharacteristic(Agreement.getEncryptData(bArr));
        }
    }

    private void sendSendDataCommand(BleDevice bleDevice, int i, int i2) {
        if (bleDevice == null) {
            return;
        }
        byte[] bArrInt2Bytes = Agreement.int2Bytes(i);
        byte[] encryptData = Agreement.getEncryptData(new byte[]{9, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 83, bArrInt2Bytes[0], bArrInt2Bytes[1], 0, (byte) i2, 1, 0, 0, 0, 0, 0, 0});
        LogUtil.d("发送的数据11：" + ByteUtils.binaryToHexString(encryptData));
        bleDevice.writeCharacteristic(encryptData);
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
    public void sendSendDataDatcpCommand(BleDevice bleDevice, byte[] bArr) {
        if (bleDevice == null || bArr == null || bArr.length < 4) {
            return;
        }
        bleDevice.writeCharacteristic(Agreement.getEncryptData(new byte[]{9, JSONB.Constants.BC_INT32_SHORT_ZERO, 65, 84, 67, 80, bArr[0], bArr[1], bArr[2], bArr[3], 0, 0, 0, 0, 0, 0}));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isFaceOk(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 70 && bArr[2] == 65 && bArr[3] == 67 && bArr[4] == 69 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isDatcpOk(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && bArr[1] == 68 && bArr[2] == 65 && bArr[3] == 84 && bArr[4] == 67 && bArr[5] == 80 && bArr[6] == 79 && bArr[7] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isDeleOk(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 68 && bArr[2] == 69 && bArr[3] == 76 && bArr[4] == 69 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isPlayOk(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 80 && bArr[2] == 76 && bArr[3] == 65 && bArr[4] == 89 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isChec(byte[] bArr) {
        return bArr != null && 5 <= bArr.length && bArr[1] == 67 && bArr[2] == 72 && bArr[3] == 69 && bArr[4] == 67;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isTimeOk(byte[] bArr) {
        return bArr != null && 7 <= bArr.length && bArr[1] == 84 && bArr[2] == 73 && bArr[3] == 77 && bArr[4] == 69 && bArr[5] == 79 && bArr[6] == 75;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isTimeErr(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && bArr[1] == 84 && bArr[2] == 73 && bArr[3] == 77 && bArr[4] == 69 && bArr[5] == 69 && bArr[6] == 82 && bArr[7] == 82;
    }

    class BleListenerImpl extends BleListener {
        BleListenerImpl() {
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onChanged(BleDevice bleDevice, byte[] bArr) {
            System.arraycopy(bArr, 0, new byte[bArr.length], 0, bArr.length);
            byte[] decodeData = Agreement.getDecodeData(bArr);
            StringBuffer stringBuffer = new StringBuffer();
            for (byte b : decodeData) {
                stringBuffer.append((char) b);
            }
            LogUtil.d("MCU回复App:" + bleDevice.getBleName() + " " + stringBuffer.toString());
            if (DiyAgreement.this.listenerMap.containsKey(bleDevice)) {
                if (DiyAgreement.this.parseSendDataCommand(decodeData)) {
                    TextData textData = new TextData(0, DiyAgreement.this.getSendData(bleDevice, DiyAgreement.this.dataMap.get(bleDevice)));
                    DiyAgreement.this.dataMap1.put(bleDevice, textData);
                    DiyAgreement.this.sendTextData(bleDevice, textData.getDataList().get(textData.getCurIndex()));
                    return;
                }
                if (DiyAgreement.this.isResultReOk(decodeData)) {
                    LogUtil.d("返回REOK");
                    TextData textData2 = DiyAgreement.this.dataMap1.get(bleDevice);
                    if (textData2 == null) {
                        return;
                    }
                    List<byte[]> dataList = textData2.getDataList();
                    int curIndex = textData2.getCurIndex();
                    if (curIndex < dataList.size() - 1) {
                        int i = curIndex + 1;
                        textData2.setCurIndex(i);
                        DiyAgreement.this.sendTextData(bleDevice, dataList.get(i));
                        return;
                    } else {
                        if (curIndex == dataList.size() - 1) {
                            LogUtil.d("发送的时间:" + ByteUtils.binaryToHexString(ByteUtils.intToByteArrayH(DiyAgreement.this.cropImage.getTimeInt())));
                            DiyAgreement diyAgreement = DiyAgreement.this;
                            diyAgreement.sendSendDataDatcpCommand(bleDevice, ByteUtils.intToByteArrayH(diyAgreement.cropImage.getTimeInt()));
                            return;
                        }
                        return;
                    }
                }
                if (DiyAgreement.this.isFaceOk(decodeData)) {
                    LogUtil.d("返回FACEOK");
                    DiyAgreement.this.faceOk(bleDevice);
                    return;
                }
                if (DiyAgreement.this.isError(decodeData)) {
                    LogUtil.d("返回ERROR=");
                    DiyAgreement.this.sendDataFinish(bleDevice);
                    DiyAgreement.this.clearDataByDevice(bleDevice);
                    return;
                }
                if (DiyAgreement.this.isDatcpOk(decodeData)) {
                    LogUtil.d("返回DATCPOK");
                    DiyAgreement.this.sendDataFinish(bleDevice);
                    return;
                }
                if (DiyAgreement.this.isChec(decodeData)) {
                    LogUtil.d("返回Chec：" + ((int) decodeData[5]));
                    DiyAgreement.this.getDiyImageCountOk(bleDevice, decodeData[5]);
                    DiyAgreement.this.clearDataByDevice(bleDevice);
                    return;
                }
                if (DiyAgreement.this.isDeleOk(decodeData)) {
                    LogUtil.d("返回DELEOK");
                    DiyAgreement.this.deleteImageOk(bleDevice);
                    DiyAgreement.this.clearDataByDevice(bleDevice);
                } else if (DiyAgreement.this.isPlayOk(decodeData)) {
                    LogUtil.d("返回PLAYOK");
                    DiyAgreement.this.playDiyImageOk(bleDevice);
                } else if (DiyAgreement.this.isTimeOk(decodeData)) {
                    DiyAgreement.this.isSynDiyImage(bleDevice, false);
                    LogUtil.d("返回TimeOk");
                } else if (DiyAgreement.this.isTimeErr(decodeData)) {
                    DiyAgreement.this.isSynDiyImage(bleDevice, true);
                    LogUtil.d("返回TimeErr");
                }
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isError(byte[] bArr) {
        return bArr != null && 8 <= bArr.length && ((char) bArr[1]) == 'E' && ((char) bArr[2]) == 'R' && ((char) bArr[3]) == 'R' && ((char) bArr[4]) == 'O' && ((char) bArr[5]) == 'R';
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
            instance = null;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void clearDataByDevice(BleDevice bleDevice) {
        this.dataMap.remove(bleDevice);
        this.dataMap1.remove(bleDevice);
        this.listenerMap.remove(bleDevice);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendDataFinish(BleDevice bleDevice) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onFinishSend(bleDevice);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void deleteImageOk(BleDevice bleDevice) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onDeleteDiyImageOk(bleDevice);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void getDiyImageCountOk(BleDevice bleDevice, int i) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onGetDiyImageCountOk(i);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void isSynDiyImage(BleDevice bleDevice, boolean z) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onIsSynDiyImage(z);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void playDiyImageOk(BleDevice bleDevice) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onPlayDiyImageOk(bleDevice);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void faceOk(BleDevice bleDevice) {
        DiyAgreementListener diyAgreementListener = this.listenerMap.get(bleDevice);
        if (diyAgreementListener != null) {
            diyAgreementListener.onFaceOk(bleDevice);
        }
    }
}
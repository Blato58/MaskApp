package cn.com.heaton.shiningmask.model.data;

import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.alibaba.fastjson2.JSONB;
import csh.tiro.cc.aes;
import java.util.Random;

/* JADX INFO: loaded from: classes.dex */
public class Agreement {
    public static byte[] getEnterDiyCommand() {
        return new byte[]{6, 83, JSONB.Constants.BC_STR_ASCII_FIX_4, 86, 69, 87, 1, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getEnterDiyCommand1() {
        return new byte[]{6, 83, JSONB.Constants.BC_STR_ASCII_FIX_4, 86, 69, 87, 3, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getExitDiyCommand() {
        LogUtil.d("退出diy时没有数据");
        return new byte[]{6, 83, JSONB.Constants.BC_STR_ASCII_FIX_4, 86, 69, 87, 0, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getExitDiySaveCommand() {
        LogUtil.d("退出diy时有数据");
        return new byte[]{6, 83, JSONB.Constants.BC_STR_ASCII_FIX_4, 86, 69, 87, 2, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getExitRhyCommand() {
        return new byte[]{4, 83, 79, 85, 84, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getContentCommand(int i) {
        LogUtil.d("发送字体动画模式：" + i);
        return new byte[]{5, JSONB.Constants.BC_STR_ASCII_FIX_4, 79, JSONB.Constants.BC_INT32_SHORT_ZERO, 69, (byte) i, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getTextColor(byte b, byte b2, byte b3, boolean z) {
        return new byte[]{6, 70, 67, z ? (byte) 1 : (byte) 0, b, b2, b3, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getTextBgColor(byte b, byte b2, byte b3, boolean z) {
        byte[] bArr = {6, 66, 67, z ? (byte) 1 : (byte) 0, b, b2, b3, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
        LogUtil.d("发送的文本背景颜色：" + ByteUtils.binaryToHexString(bArr));
        return bArr;
    }

    public static byte[] getDefaultMode(int i, boolean z) {
        byte[] bArr = {3, JSONB.Constants.BC_STR_ASCII_FIX_4, z ? (byte) 1 : (byte) 0, (byte) i, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
        LogUtil.d("得到预设颜色模式命令:" + ByteUtils.binaryToHexString(bArr));
        return bArr;
    }

    public static byte[] getSpeed(int i) {
        byte b = (byte) i;
        LogUtil.e("speed:" + ((int) b));
        return new byte[]{6, 83, 80, 69, 69, JSONB.Constants.BC_INT32_SHORT_ZERO, b, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getLight(int i) {
        byte b = (byte) i;
        LogUtil.e("light:" + ((int) b));
        return new byte[]{6, 76, 73, JSONB.Constants.BC_INT32_SHORT_MAX, JSONB.Constants.BC_INT32, 84, b, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getAnimLoopCommand() {
        LogUtil.e("得到发送动画循环命令:");
        return new byte[]{4, 76, 79, 79, 65, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getAnimCommand(int i) {
        byte b = (byte) i;
        LogUtil.e("发送动画命令:" + ((int) b));
        return new byte[]{5, 65, JSONB.Constants.BC_STR_ASCII_FIX_5, 73, JSONB.Constants.BC_STR_ASCII_FIX_4, b, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getImageCommand(int i) {
        byte b = (byte) i;
        LogUtil.e("得到发送图片命令:" + ((int) b));
        return new byte[]{5, 73, JSONB.Constants.BC_STR_ASCII_FIX_4, 65, JSONB.Constants.BC_INT32_SHORT_MAX, b, getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom(), getRandom()};
    }

    public static byte[] getDiyImageCommand(int i) {
        byte b = (byte) i;
        LogUtil.e("得到发送diy图片命令:" + ((int) b));
        return new byte[]{4, JSONB.Constants.BC_INT32_SHORT_ZERO, 73, 89, b, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    public static byte[] getGestureSettings(byte b, byte[] bArr, byte b2, byte b3, byte b4) {
        LogUtil.e("手势变脸设置命令:" + ((int) ((byte) bArr.length)));
        if (b2 == 1) {
            byte length = bArr.length > 7 ? (byte) 8 : (byte) bArr.length;
            byte b5 = b4 == 0 ? (byte) 0 : b;
            byte[] bArr2 = new byte[16];
            bArr2[0] = (byte) (length + 8);
            bArr2[1] = 70;
            bArr2[2] = 65;
            bArr2[3] = 67;
            bArr2[4] = 69;
            bArr2[5] = b2;
            bArr2[6] = b3;
            bArr2[7] = b4;
            bArr2[8] = b5;
            bArr2[9] = 0;
            bArr2[10] = 0;
            bArr2[11] = 0;
            bArr2[12] = 0;
            bArr2[13] = 0;
            bArr2[14] = 0;
            bArr2[15] = 0;
            if (b4 > 0 && bArr != null && bArr.length > 0) {
                if (bArr.length > 7) {
                    for (int i = 0; i < 7; i++) {
                        bArr2[i + 9] = bArr[i];
                    }
                } else {
                    for (int i2 = 0; i2 < bArr.length; i2++) {
                        bArr2[i2 + 9] = bArr[i2];
                    }
                }
            }
            return bArr2;
        }
        return new byte[]{8, 70, 65, 67, 69, 0, b3, b4, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    public static byte[] getGestureSettings2(byte[] bArr) {
        LogUtil.e("手势变脸设置2:" + ((int) ((byte) bArr.length)));
        byte[] bArr2 = new byte[16];
        int i = 0;
        bArr2[0] = (byte) bArr.length;
        bArr2[1] = 0;
        bArr2[2] = 0;
        bArr2[3] = 0;
        bArr2[4] = 0;
        bArr2[5] = 0;
        bArr2[6] = 0;
        bArr2[7] = 0;
        bArr2[8] = 0;
        bArr2[9] = 0;
        bArr2[10] = 0;
        bArr2[11] = 0;
        bArr2[12] = 0;
        bArr2[13] = 0;
        bArr2[14] = 0;
        bArr2[15] = 0;
        while (i < bArr.length) {
            int i2 = i + 1;
            bArr2[i2] = bArr[i];
            i = i2;
        }
        return bArr2;
    }

    public static byte[] getDiyImageTime(byte b, int i) {
        byte[] bArrIntToByteArrayH = ByteUtils.intToByteArrayH(i);
        LogUtil.e("查询DIY图片时间戳:" + ByteUtils.binaryToHexString(bArrIntToByteArrayH));
        return new byte[]{9, 84, 73, JSONB.Constants.BC_STR_ASCII_FIX_4, 69, b, bArrIntToByteArrayH[0], bArrIntToByteArrayH[1], bArrIntToByteArrayH[2], bArrIntToByteArrayH[3], 0, 0, 0, 0, 0, 0};
    }

    public static byte[] getPlayDiyImage(int i, byte[] bArr) {
        LogUtil.e("播放DIY图片命令:" + ((int) ((byte) bArr.length)) + " data:" + ByteUtils.binaryToHexString(bArr));
        byte[] bArr2 = new byte[16];
        int i2 = 0;
        bArr2[0] = bArr.length > 10 ? (byte) 19 : (byte) (((byte) bArr.length) + 5);
        bArr2[1] = 80;
        bArr2[2] = 76;
        bArr2[3] = 65;
        bArr2[4] = 89;
        bArr2[5] = (byte) i;
        bArr2[6] = 0;
        bArr2[7] = 0;
        bArr2[8] = 0;
        bArr2[9] = 0;
        bArr2[10] = 0;
        bArr2[11] = 0;
        bArr2[12] = 0;
        bArr2[13] = 0;
        bArr2[14] = 0;
        bArr2[15] = 0;
        if (bArr != null && bArr.length > 0) {
            if (bArr.length > 10) {
                while (i2 < 10) {
                    bArr2[i2 + 6] = bArr[i2];
                    i2++;
                }
            } else {
                while (i2 < bArr.length) {
                    bArr2[i2 + 6] = bArr[i2];
                    i2++;
                }
            }
        }
        return bArr2;
    }

    public static byte[] getPlayDiyImage2(byte[] bArr) {
        LogUtil.e("播放DIY图片命令2:" + ((int) ((byte) bArr.length)));
        byte[] bArr2 = new byte[16];
        bArr2[0] = (byte) (((byte) bArr.length) - 10);
        bArr2[1] = 0;
        bArr2[2] = 0;
        bArr2[3] = 0;
        bArr2[4] = 0;
        bArr2[5] = 0;
        bArr2[6] = 0;
        bArr2[7] = 0;
        bArr2[8] = 0;
        bArr2[9] = 0;
        bArr2[10] = 0;
        bArr2[11] = 0;
        bArr2[12] = 0;
        bArr2[13] = 0;
        bArr2[14] = 0;
        bArr2[15] = 0;
        if (bArr != null && bArr.length > 10) {
            for (int i = 10; i < bArr.length; i++) {
                bArr2[i - 9] = bArr[i];
            }
        }
        return bArr2;
    }

    public static byte[] getDeleteDiyImage(int i, byte[] bArr) {
        LogUtil.e("删除DIY图片数量命令:" + ((int) ((byte) bArr.length)));
        byte[] bArr2 = new byte[16];
        int i2 = 0;
        bArr2[0] = bArr.length > 10 ? (byte) 15 : (byte) (((byte) bArr.length) + 5);
        bArr2[1] = JSONB.Constants.BC_INT32_SHORT_ZERO;
        bArr2[2] = 69;
        bArr2[3] = 76;
        bArr2[4] = 69;
        bArr2[5] = (byte) i;
        bArr2[6] = 0;
        bArr2[7] = 0;
        bArr2[8] = 0;
        bArr2[9] = 0;
        bArr2[10] = 0;
        bArr2[11] = 0;
        bArr2[12] = 0;
        bArr2[13] = 0;
        bArr2[14] = 0;
        bArr2[15] = 0;
        if (bArr != null && bArr.length > 0) {
            if (bArr.length <= 10) {
                while (i2 < bArr.length) {
                    bArr2[i2 + 6] = bArr[i2];
                    i2++;
                }
            } else {
                while (i2 < 10) {
                    bArr2[i2 + 6] = bArr[i2];
                    i2++;
                }
            }
        }
        return bArr2;
    }

    public static byte[] getDeleteDiyImage2(byte[] bArr) {
        LogUtil.e("删除DIY图片图片数量命令:" + ((int) ((byte) bArr.length)));
        byte[] bArr2 = new byte[16];
        bArr2[0] = (byte) (((byte) bArr.length) - 10);
        bArr2[1] = 0;
        bArr2[2] = 0;
        bArr2[3] = 0;
        bArr2[4] = 0;
        bArr2[5] = 0;
        bArr2[6] = 0;
        bArr2[7] = 0;
        bArr2[8] = 0;
        bArr2[9] = 0;
        bArr2[10] = 0;
        bArr2[11] = 0;
        bArr2[12] = 0;
        bArr2[13] = 0;
        bArr2[14] = 0;
        bArr2[15] = 0;
        if (bArr != null && bArr.length > 10) {
            for (int i = 10; i < bArr.length; i++) {
                bArr2[i - 9] = bArr[i];
            }
        }
        return bArr2;
    }

    public static byte[] getDiyImageCount() {
        LogUtil.e("获取DIY图片图片数量命令:");
        return new byte[]{4, 67, JSONB.Constants.BC_INT32, 69, 67, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    public static byte[] int2Bytes(int i) {
        return new byte[]{(byte) (i / 256), (byte) (i % 256)};
    }

    public static byte[] getEncryptData(byte[] bArr) {
        aes.cipher(bArr, bArr);
        return bArr;
    }

    public static byte[] getDecodeData(byte[] bArr) {
        aes.invCipher(bArr, bArr);
        return bArr;
    }

    public static byte getRandom() {
        return (byte) (new Random().nextInt(256) & 255);
    }
}
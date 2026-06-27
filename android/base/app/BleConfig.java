package cn.com.heaton.shiningmask.base.app;

import com.alibaba.fastjson2.JSONB;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class BleConfig {
    public static final int BLE_GAP_AD_TYPE_MANUFACTURER_SPECIFIC_DATA = 255;
    public static final byte[] BROADCAST_SPECIFIC1_5_PRODUCT = {84, 82, 0, JSONB.Constants.BC_STR_ASCII_FIX_1};

    public static int matchProduct(byte[] bArr) {
        int i;
        if (bArr != null && bArr.length > 0) {
            int i2 = 0;
            do {
                int i3 = i2 + 1;
                int i4 = bArr[i2] & UByte.MAX_VALUE;
                if (i4 <= 0) {
                    i2 = i3;
                } else {
                    if (i4 > 31) {
                        return -1;
                    }
                    byte[] bArr2 = new byte[i4];
                    int i5 = 0;
                    while (true) {
                        int i6 = i5 + 1;
                        i = i3 + 1;
                        bArr2[i5] = bArr[i3];
                        if (i6 >= i4) {
                            break;
                        }
                        i5 = i6;
                        i3 = i;
                    }
                    if (i4 > BROADCAST_SPECIFIC1_5_PRODUCT.length && (bArr2[0] & UByte.MAX_VALUE) == 255) {
                        int i7 = 0;
                        boolean z = true;
                        while (true) {
                            byte[] bArr3 = BROADCAST_SPECIFIC1_5_PRODUCT;
                            if (i7 >= bArr3.length) {
                                break;
                            }
                            z = z && bArr2[i7 + 1] == bArr3[i7];
                            i7++;
                        }
                        if (z) {
                            return 0;
                        }
                    }
                    i2 = i;
                }
            } while (i2 < bArr.length);
        }
        return -1;
    }
}
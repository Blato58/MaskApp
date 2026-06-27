package cn.com.heaton.shiningmask.base.music;

import csh.tiro.cc.fft.int16FFT;

/* JADX INFO: loaded from: classes.dex */
public class VisualizerUtil {
    private static short _maxLevel = 0;
    private static short _minLevel = Short.MAX_VALUE;
    static short maxvalue = Short.MIN_VALUE;

    public static byte[] getWaveFormData(short[] sArr) {
        for (short s : sArr) {
            if (s > maxvalue) {
                maxvalue = s;
            }
        }
        short[] sArr2 = new short[128];
        short[] sArr3 = new short[128];
        if (sArr.length > 128) {
            System.arraycopy(sArr, 0, sArr2, 0, 128);
            int16FFT.WindowCalc(sArr2, (char) 0);
            int16FFT.BitReverse(sArr2);
            int16FFT.IntFFT(sArr2, sArr3);
        }
        short[] sArr4 = new short[28];
        for (int i = 3; i < int16FFT.NUM_FFT / 2 && i < 31; i++) {
            short s2 = sArr2[i];
            double d = ((double) s2) * ((double) s2);
            short s3 = sArr3[i];
            short sSqrt = (short) Math.sqrt(d + (((double) s3) * ((double) s3)));
            sArr4[i - 3] = sSqrt;
            if (sSqrt > _maxLevel) {
                _maxLevel = sSqrt;
            }
            if (sSqrt < _minLevel) {
                _minLevel = sSqrt;
            }
        }
        short s4 = (short) (((double) _maxLevel) * 0.98d);
        _maxLevel = s4;
        float fAbs = Math.abs(s4 - _minLevel) / 9;
        if (fAbs < 1.0d) {
            fAbs = 1.0f;
        }
        for (int i2 = 0; i2 < 28; i2++) {
            if (i2 == 0 && sArr4[i2] / fAbs < 0.5d) {
                sArr4[i2] = 0;
            } else {
                sArr4[i2] = (short) Math.ceil(sArr4[i2] / fAbs);
            }
            if (sArr4[i2] >= 9) {
                sArr4[i2] = 9;
            }
        }
        byte[] bArr = new byte[14];
        for (int i3 = 0; i3 < 14; i3++) {
            int i4 = i3 * 2;
            bArr[i3] = (byte) (sArr4[i4 + 1] | (sArr4[i4] << 4));
        }
        byte[] bArr2 = new byte[16];
        System.arraycopy(bArr, 0, bArr2, 0, 14);
        return bArr2;
    }
}
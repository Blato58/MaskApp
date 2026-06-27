package cn.com.heaton.shiningmask.base.music;

import android.media.audiofx.Visualizer;
import cn.com.heaton.shiningmask.sevice.MusicFftListenter;
import csh.tiro.cc.fft.int16FFT;
import kotlin.jvm.internal.ByteCompanionObject;
import kotlin.jvm.internal.ShortCompanionObject;

/* JADX INFO: loaded from: classes.dex */
public class VisualizerView implements Visualizer.OnDataCaptureListener {
    static boolean isExit;
    private short _maxLevel = 0;
    private short _minLevel = ShortCompanionObject.MAX_VALUE;
    public MusicFftListenter musicFftListenter;

    @Override // android.media.audiofx.Visualizer.OnDataCaptureListener
    public void onFftDataCapture(Visualizer visualizer, byte[] bArr, int i) {
    }

    VisualizerView() {
    }

    public static void setIsExit(boolean z) {
        isExit = z;
    }

    @Override // android.media.audiofx.Visualizer.OnDataCaptureListener
    public void onWaveFormDataCapture(Visualizer visualizer, byte[] bArr, int i) {
        int length = bArr.length;
        byte[] bArr2 = new byte[length];
        System.arraycopy(bArr, 0, bArr2, 0, bArr.length);
        for (int i2 = 0; i2 < length; i2++) {
            bArr2[i2] = (byte) (bArr2[i2] - ByteCompanionObject.MIN_VALUE);
        }
        short[] sArr = new short[128];
        short[] sArr2 = new short[128];
        for (int i3 = 0; i3 < int16FFT.NUM_FFT && i3 < length; i3++) {
            sArr[i3] = (short) (bArr2[i3] * 256);
        }
        int16FFT.WindowCalc(sArr, (char) 0);
        int16FFT.BitReverse(sArr);
        int16FFT.IntFFT(sArr, sArr2);
        short[] sArr3 = new short[28];
        for (int i4 = 3; i4 < int16FFT.NUM_FFT / 2 && i4 < 31; i4++) {
            short s = sArr[i4];
            double d = ((double) s) * ((double) s);
            short s2 = sArr2[i4];
            short sSqrt = (short) Math.sqrt(d + (((double) s2) * ((double) s2)));
            sArr3[i4 - 3] = sSqrt;
            if (sSqrt > this._maxLevel) {
                this._maxLevel = sSqrt;
            }
            if (sSqrt < this._minLevel) {
                this._minLevel = sSqrt;
            }
        }
        short s3 = (short) (((double) this._maxLevel) * 0.98d);
        this._maxLevel = s3;
        float fAbs = Math.abs(s3 - this._minLevel) / 9;
        if (fAbs < 1.0d) {
            fAbs = 1.0f;
        }
        for (int i5 = 0; i5 < 28; i5++) {
            if (i5 == 0 && sArr3[i5] / fAbs < 0.5d) {
                sArr3[i5] = 0;
            } else {
                sArr3[i5] = (short) Math.ceil(sArr3[i5] / fAbs);
            }
            if (sArr3[i5] >= 9) {
                sArr3[i5] = 9;
            }
        }
        byte[] bArr3 = new byte[14];
        for (int i6 = 0; i6 < 14; i6++) {
            int i7 = i6 * 2;
            bArr3[i6] = (byte) (sArr3[i7 + 1] | (sArr3[i7] << 4));
        }
        byte[] bArr4 = new byte[16];
        System.arraycopy(bArr3, 0, bArr4, 0, 14);
        MusicFftListenter musicFftListenter = this.musicFftListenter;
        if (musicFftListenter != null) {
            musicFftListenter.onStart(bArr4);
        }
    }

    public void setMusicFftListenter(MusicFftListenter musicFftListenter) {
        this.musicFftListenter = musicFftListenter;
    }
}
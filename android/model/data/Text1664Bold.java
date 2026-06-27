package cn.com.heaton.shiningmask.model.data;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import cn.com.heaton.shiningmask.ui.utils.FontUtil;
import com.alibaba.fastjson2.JSONB;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.Array;
import java.util.LinkedList;
import kotlin.jvm.internal.ByteCompanionObject;

/* JADX INFO: loaded from: classes.dex */
public class Text1664Bold {
    private static int textColor = Color.parseColor("#FFFFFF");

    public static byte[] getStringBytes(String str) throws UnsupportedEncodingException {
        LinkedList linkedList = new LinkedList();
        int length = 0;
        for (char c : str.toCharArray()) {
            byte[] charDataByFont = getCharDataByFont(c);
            if (charDataByFont == null) {
                charDataByFont = getCharDataByBitmap(c);
            }
            linkedList.add(charDataByFont);
            length += charDataByFont.length;
        }
        byte[] bArr = new byte[length];
        int length2 = 0;
        for (int i = 0; i < linkedList.size(); i++) {
            byte[] bArr2 = (byte[]) linkedList.get(i);
            System.arraycopy(bArr2, 0, bArr, length2, bArr2.length);
            length2 += bArr2.length;
        }
        return bArr;
    }

    private static byte[] getCharDataByBitmap(char c) {
        return getCharData(getCharBitmapPointCheckData(getCharBitmap(c)));
    }

    private static Bitmap getCharBitmap(char c) {
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(16, 16, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        Paint paint = new Paint();
        double d = 16;
        paint.setTextSize((float) (1.036d * d));
        paint.setColor(textColor);
        paint.setTypeface(FontUtil.getTypefaceBold1664());
        paint.setStyle(Paint.Style.FILL);
        paint.setTextAlign(Paint.Align.CENTER);
        canvas.drawText(String.valueOf(c), 7 + 1.0f, (float) ((d * 5.39d) / 6.0d), paint);
        return bitmapCreateBitmap;
    }

    private static byte[][] getCharBitmapPointCheckData(Bitmap bitmap) {
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        byte[][] bArr = (byte[][]) Array.newInstance((Class<?>) Byte.TYPE, width, height);
        for (int i = 0; i < width; i++) {
            for (int i2 = 0; i2 < height; i2++) {
                if (bitmap.getPixel(i, i2) < 0) {
                    bArr[i][i2] = 1;
                }
            }
        }
        return bArr;
    }

    private static byte[] getCharData(byte[][] bArr) {
        int i;
        int i2;
        byte[] bArr2 = new byte[32];
        for (int i3 = 0; i3 < bArr.length; i3++) {
            byte[] bArr3 = bArr[i3];
            byte b = 0;
            byte b2 = 0;
            for (int i4 = 0; i4 < bArr3.length; i4++) {
                if (bArr3[i4] == 1) {
                    switch (i4) {
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
                        case 12:
                            i2 = b2 | 8;
                            b2 = (byte) i2;
                            break;
                        case 13:
                            i2 = b2 | 4;
                            b2 = (byte) i2;
                            break;
                        case 14:
                            i2 = b2 | 2;
                            b2 = (byte) i2;
                            break;
                        case 15:
                            i2 = b2 | 1;
                            b2 = (byte) i2;
                            break;
                    }
                }
            }
            int i5 = i3 * 2;
            bArr2[i5] = b;
            bArr2[i5 + 1] = b2;
        }
        return bArr2;
    }

    private static byte[] getCharDataByFont(char c) {
        if (c == 193) {
            return m105get_pt_();
        }
        if (c == 195) {
            return m107get_pt_();
        }
        if (c == 192) {
            return m104get_pt_();
        }
        if (c == 194) {
            return m106get_pt_();
        }
        if (c == 199) {
            return m108get_pt_();
        }
        if (c == 201) {
            return m110get_pt_();
        }
        if (c == 200) {
            return m109get_pt_();
        }
        if (c == 202) {
            return m111get_pt_();
        }
        if (c == 204) {
            return m112get_pt_();
        }
        if (c == 205) {
            return m113get_pt_();
        }
        if (c == 206) {
            return m114get_pt_();
        }
        if (c == 210) {
            return m115get_pt_();
        }
        if (c == 211) {
            return m116get_pt_();
        }
        if (c == 212) {
            return m117get_pt_();
        }
        if (c == 213) {
            return m118get_pt_();
        }
        if (c == 218) {
            return m120get_pt_();
        }
        if (c == 217) {
            return m119get_pt_();
        }
        if (c == 219) {
            return m121get_pt_();
        }
        if (c == 224) {
            return m122get_pt_();
        }
        if (c == 225) {
            return m123get_pt_();
        }
        if (c == 227) {
            return m124get_pt_();
        }
        if (c == 231) {
            return m125get_pt_();
        }
        if (c == 234) {
            return m127get_pt_();
        }
        if (c == 233) {
            return m126get_pt_();
        }
        if (c == 243) {
            return m129get_pt_();
        }
        if (c == 245) {
            return m130get_pt_();
        }
        if (c == 237) {
            return m128get_pt_();
        }
        if (c == 250) {
            return m131get_pt_();
        }
        if (c == 'A') {
            return get_A();
        }
        if (c == 'B') {
            return get_B();
        }
        if (c == 'C') {
            return get_C();
        }
        if (c == 'D') {
            return get_D();
        }
        if (c == 'E') {
            return get_E();
        }
        if (c == 'F') {
            return get_F();
        }
        if (c == 'G') {
            return get_G();
        }
        if (c == 'H') {
            return get_H();
        }
        if (c == 'I') {
            return get_I();
        }
        if (c == 'J') {
            return get_J();
        }
        if (c == 'K') {
            return get_K();
        }
        if (c == 'L') {
            return get_L();
        }
        if (c == 'M') {
            return get_M();
        }
        if (c == 'N') {
            return get_N();
        }
        if (c == 'O') {
            return get_O();
        }
        if (c == 'P') {
            return get_P();
        }
        if (c == 'Q') {
            return get_Q();
        }
        if (c == 'R') {
            return get_R();
        }
        if (c == 'S') {
            return get_S();
        }
        if (c == 'T') {
            return get_T();
        }
        if (c == 'U') {
            return get_U();
        }
        if (c == 'V') {
            return get_V();
        }
        if (c == 'W') {
            return get_W();
        }
        if (c == 'X') {
            return get_X();
        }
        if (c == 'Y') {
            return get_Y();
        }
        if (c == 'Z') {
            return get_Z();
        }
        if (c == '0') {
            return get_0();
        }
        if (c == '1') {
            return get_1();
        }
        if (c == '2') {
            return get_2();
        }
        if (c == '3') {
            return get_3();
        }
        if (c == '4') {
            return get_4();
        }
        if (c == '5') {
            return get_5();
        }
        if (c == '6') {
            return get_6();
        }
        if (c == '7') {
            return get_7();
        }
        if (c == '8') {
            return get_8();
        }
        if (c == '9') {
            return get_9();
        }
        if (c == 'a') {
            return get_a();
        }
        if (c == 'b') {
            return get_b();
        }
        if (c == 'c') {
            return get_c();
        }
        if (c == 'd') {
            return get_d();
        }
        if (c == 'e') {
            return get_e();
        }
        if (c == 'f') {
            return get_f();
        }
        if (c == 'g') {
            return get_g();
        }
        if (c == 'h') {
            return get_h();
        }
        if (c == 'i') {
            return get_i();
        }
        if (c == 'j') {
            return get_j();
        }
        if (c == 'k') {
            return get_k();
        }
        if (c == 'l') {
            return get_l();
        }
        if (c == 'm') {
            return get_m();
        }
        if (c == 'n') {
            return get_n();
        }
        if (c == 'o') {
            return get_o();
        }
        if (c == 'p') {
            return get_p();
        }
        if (c == 'q') {
            return get_q();
        }
        if (c == 'r') {
            return get_r();
        }
        if (c == 's') {
            return get_s();
        }
        if (c == 't') {
            return get_t();
        }
        if (c == 'u') {
            return get_u();
        }
        if (c == 'v') {
            return get_v();
        }
        if (c == 'w') {
            return get_w();
        }
        if (c == 'x') {
            return get_x();
        }
        if (c == 'y') {
            return get_y();
        }
        if (c == 'z') {
            return get_z();
        }
        if (c == '<') {
            return get_left();
        }
        if (c == '>') {
            return get_right();
        }
        if (c == ',') {
            return get_comma();
        }
        if (c == '.') {
            return get_period();
        }
        if (c == ';') {
            return get_fenhao();
        }
        if (c == ':') {
            return get_maohao();
        }
        if (c == '\'') {
            return get_suoxiehao();
        }
        if (c == '\"') {
            return get_shuangsuoxiehao();
        }
        if (c == '[') {
            return get_zuozhongkuohao();
        }
        if (c == ']') {
            return get_youzhongkuohao();
        }
        if (c == '{') {
            return get_zuodakuohao();
        }
        if (c == '}') {
            return get_youdakuohao();
        }
        if (c == '|') {
            return get_shuxian();
        }
        if (c == '\\') {
            return get_fanxiegang();
        }
        if (c == '/') {
            return get_xiegang();
        }
        if (c == '?') {
            return get_question();
        }
        if (c == '~') {
            return get_pozhehao();
        }
        if (c == '`') {
            return get_piedian();
        }
        if (c == '!') {
            return get_exclamation();
        }
        if (c == '@') {
            return get_xiaolaoshu();
        }
        if (c == '#') {
            return get_jinghao();
        }
        if (c == '$') {
            return get_meiyuanfuhao();
        }
        if (c == 65509) {
            return get_renminbifuhao();
        }
        if (c == '%') {
            return get_baifenbi();
        }
        if (c == '^') {
            return get_yunsuanfuhao();
        }
        if (c == '&') {
            return get_yufuhao();
        }
        if (c == '*') {
            return get_xinghao();
        }
        if (c == '(') {
            return get_zuokuohao();
        }
        if (c == ')') {
            return get_youkuohao();
        }
        if (c == '-') {
            return get_dash();
        }
        if (c == '_') {
            return get_xiahuaxian();
        }
        if (c == '+') {
            return get_add();
        }
        if (c == '=') {
            return get_equal();
        }
        if (c == ' ') {
            return get_space();
        }
        return null;
    }

    /* JADX INFO: renamed from: get_pt_Á, reason: contains not printable characters */
    private static byte[] m105get_pt_() {
        return new byte[]{0, 0, 1, JSONB.Constants.BC_INT32_NUM_MIN, JSONB.Constants.BC_STR_ASCII_FIX_5, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_FALSE, ByteCompanionObject.MIN_VALUE, 14, ByteCompanionObject.MIN_VALUE, 1, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ã, reason: contains not printable characters */
    private static byte[] m107get_pt_() {
        return new byte[]{65, JSONB.Constants.BC_INT32_NUM_MIN, -114, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_FALSE, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_STR_ASCII_FIX_5, ByteCompanionObject.MIN_VALUE, 65, JSONB.Constants.BC_INT32_NUM_MIN, ByteCompanionObject.MIN_VALUE, 0, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_À, reason: contains not printable characters */
    private static byte[] m104get_pt_() {
        return new byte[]{0, 0, 1, JSONB.Constants.BC_INT32_NUM_MIN, 14, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_FALSE, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_STR_ASCII_FIX_5, ByteCompanionObject.MIN_VALUE, 1, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Â, reason: contains not printable characters */
    private static byte[] m106get_pt_() {
        return new byte[]{0, 0, 1, JSONB.Constants.BC_INT32_NUM_MIN, 70, ByteCompanionObject.MIN_VALUE, -104, ByteCompanionObject.MIN_VALUE, 70, ByteCompanionObject.MIN_VALUE, 1, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ç, reason: contains not printable characters */
    private static byte[] m108get_pt_() {
        return new byte[]{15, JSONB.Constants.BC_INT64_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, 36, 32, 20, 32, 24, JSONB.Constants.BC_INT32_NUM_16, 32, 8, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_É, reason: contains not printable characters */
    private static byte[] m110get_pt_() {
        return new byte[]{63, JSONB.Constants.BC_INT32_NUM_MIN, 34, JSONB.Constants.BC_INT32_NUM_16, 98, JSONB.Constants.BC_INT32_NUM_16, -94, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_È, reason: contains not printable characters */
    private static byte[] m109get_pt_() {
        return new byte[]{63, JSONB.Constants.BC_INT32_NUM_MIN, 34, JSONB.Constants.BC_INT32_NUM_16, -94, JSONB.Constants.BC_INT32_NUM_16, 98, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ê, reason: contains not printable characters */
    private static byte[] m111get_pt_() {
        return new byte[]{63, JSONB.Constants.BC_INT32_NUM_MIN, 98, JSONB.Constants.BC_INT32_NUM_16, -94, JSONB.Constants.BC_INT32_NUM_16, 98, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 34, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ì, reason: contains not printable characters */
    private static byte[] m112get_pt_() {
        return new byte[]{0, 0, 0, 0, JSONB.Constants.BC_INT64_INT, JSONB.Constants.BC_INT32_NUM_MIN, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0, 0, 0, 0, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Í, reason: contains not printable characters */
    private static byte[] m113get_pt_() {
        return new byte[]{0, 0, 0, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT64_INT, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Î, reason: contains not printable characters */
    private static byte[] m114get_pt_() {
        return new byte[]{0, 0, 0, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT64_INT, JSONB.Constants.BC_INT32_NUM_MIN, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0, 0, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ò, reason: contains not printable characters */
    private static byte[] m115get_pt_() {
        return new byte[]{15, -32, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, -96, 8, 96, 8, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 15, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ó, reason: contains not printable characters */
    private static byte[] m116get_pt_() {
        return new byte[]{15, -32, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 96, 8, -96, 8, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 15, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ô, reason: contains not printable characters */
    private static byte[] m117get_pt_() {
        return new byte[]{15, -32, 80, JSONB.Constants.BC_INT32_NUM_16, -96, 8, 96, 8, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 15, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Õ, reason: contains not printable characters */
    private static byte[] m118get_pt_() {
        return new byte[]{79, -32, JSONB.Constants.BC_CHAR, JSONB.Constants.BC_INT32_NUM_16, -96, 8, 96, 8, 80, JSONB.Constants.BC_INT32_NUM_16, -113, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ú, reason: contains not printable characters */
    private static byte[] m120get_pt_() {
        return new byte[]{63, -32, 0, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_INT32_NUM_16, 0, JSONB.Constants.BC_INT32_NUM_16, 63, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Ù, reason: contains not printable characters */
    private static byte[] m119get_pt_() {
        return new byte[]{63, -32, 0, JSONB.Constants.BC_INT32_NUM_16, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, 0, JSONB.Constants.BC_INT32_NUM_16, 63, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_Û, reason: contains not printable characters */
    private static byte[] m121get_pt_() {
        return new byte[]{63, -32, JSONB.Constants.BC_INT32_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, ByteCompanionObject.MIN_VALUE, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, 0, JSONB.Constants.BC_INT32_NUM_16, 63, -32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_à, reason: contains not printable characters */
    private static byte[] m122get_pt_() {
        return new byte[]{4, 96, JSONB.Constants.BC_INT32, JSONB.Constants.BC_CHAR, 41, JSONB.Constants.BC_INT32_NUM_16, 9, JSONB.Constants.BC_INT32_NUM_16, 7, -32, 0, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_á, reason: contains not printable characters */
    private static byte[] m123get_pt_() {
        return new byte[]{4, 96, 8, JSONB.Constants.BC_CHAR, 41, JSONB.Constants.BC_INT32_NUM_16, 73, JSONB.Constants.BC_INT32_NUM_16, 7, -32, 0, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_ã, reason: contains not printable characters */
    private static byte[] m124get_pt_() {
        return new byte[]{36, 96, JSONB.Constants.BC_INT32, JSONB.Constants.BC_CHAR, 73, JSONB.Constants.BC_INT32_NUM_16, 41, JSONB.Constants.BC_INT32_NUM_16, 39, -32, JSONB.Constants.BC_INT32_SHORT_MIN, JSONB.Constants.BC_INT32_NUM_16, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_ç, reason: contains not printable characters */
    private static byte[] m125get_pt_() {
        return new byte[]{3, JSONB.Constants.BC_INT64_SHORT_MIN, 4, 32, 8, 20, 8, 20, 8, 24, 4, 32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_ê, reason: contains not printable characters */
    private static byte[] m127get_pt_() {
        return new byte[]{3, JSONB.Constants.BC_INT64_SHORT_MIN, 5, 32, 41, JSONB.Constants.BC_INT32_NUM_16, 73, JSONB.Constants.BC_INT32_NUM_16, 37, JSONB.Constants.BC_INT32_NUM_16, 3, 32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_é, reason: contains not printable characters */
    private static byte[] m126get_pt_() {
        return new byte[]{3, JSONB.Constants.BC_INT64_SHORT_MIN, 5, 32, 41, JSONB.Constants.BC_INT32_NUM_16, 73, JSONB.Constants.BC_INT32_NUM_16, 5, JSONB.Constants.BC_INT32_NUM_16, 3, 32, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_ó, reason: contains not printable characters */
    private static byte[] m129get_pt_() {
        return new byte[]{3, JSONB.Constants.BC_INT64_SHORT_MIN, 4, 32, 40, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32, JSONB.Constants.BC_INT32_NUM_16, 4, 32, 3, JSONB.Constants.BC_INT64_SHORT_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_õ, reason: contains not printable characters */
    private static byte[] m130get_pt_() {
        return new byte[]{35, JSONB.Constants.BC_INT64_SHORT_MIN, JSONB.Constants.BC_INT32_SHORT_ZERO, 32, JSONB.Constants.BC_INT32, JSONB.Constants.BC_INT32_NUM_16, 40, JSONB.Constants.BC_INT32_NUM_16, 36, 32, 67, JSONB.Constants.BC_INT64_SHORT_MIN, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_í, reason: contains not printable characters */
    private static byte[] m128get_pt_() {
        return new byte[]{0, 0, 0, 0, 32, 0, 79, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0, 0, 0};
    }

    /* JADX INFO: renamed from: get_pt_ú, reason: contains not printable characters */
    private static byte[] m131get_pt_() {
        return new byte[]{0, 0, 15, -32, 0, JSONB.Constants.BC_INT32_NUM_16, 32, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_SHORT_MIN, 32, 15, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_renminbifuhao() {
        return new byte[]{0, 0, 65, 0, 97, 0, 57, 0, 15, -32, 7, -32, 25, 0, 97, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_A() {
        return new byte[]{0, 28, 1, -4, 31, -32, JSONB.Constants.BC_STR_GB18030, 32, 31, -32, 1, -4, 0, 28, 0, 0};
    }

    private static byte[] get_B() {
        return new byte[]{127, -4, 127, -4, 65, 4, 65, 4, 99, -116, 62, -8, 28, 112, 0, 0};
    }

    private static byte[] get_C() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 63, -8, 96, 12, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 96, 12, JSONB.Constants.BC_INT32_BYTE_ZERO, JSONB.Constants.BC_INT32_BYTE_ZERO, 24, JSONB.Constants.BC_INT32_BYTE_MIN, 0, 0};
    }

    private static byte[] get_D() {
        return new byte[]{127, -4, 127, -4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 96, 12, 63, -8, 31, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_E() {
        return new byte[]{127, -4, 127, -4, 65, 4, 65, 4, 65, 4, 65, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 0, 0};
    }

    private static byte[] get_F() {
        return new byte[]{127, -4, 127, -4, 65, 0, 65, 0, 65, 0, 65, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0, 0};
    }

    private static byte[] get_G() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 63, -8, 96, 12, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, -124, 96, -116, JSONB.Constants.BC_INT32_BYTE_ZERO, -8, 0, 0};
    }

    private static byte[] get_H() {
        return new byte[]{127, -4, 127, -4, 1, 0, 1, 0, 1, 0, 127, -4, 127, -4, 0, 0};
    }

    private static byte[] get_I() {
        return new byte[]{0, 0, 0, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 127, -4, 127, -4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 0, 0, 0, 0};
    }

    private static byte[] get_J() {
        return new byte[]{0, JSONB.Constants.BC_INT32_BYTE_MIN, 0, JSONB.Constants.BC_INT32_BYTE_ZERO, 0, 12, 0, 4, 0, 12, 127, -8, 127, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_K() {
        return new byte[]{127, -4, 127, -4, 3, ByteCompanionObject.MIN_VALUE, 14, -32, JSONB.Constants.BC_INT32_BYTE_ZERO, 112, 112, 28, JSONB.Constants.BC_INT32_SHORT_MIN, 12, 0, 0};
    }

    private static byte[] get_L() {
        return new byte[]{127, -4, 127, -4, 0, 4, 0, 4, 0, 4, 0, 4, 0, 4, 0, 0};
    }

    private static byte[] get_M() {
        return new byte[]{127, -4, 127, -4, 31, JSONB.Constants.BC_INT64_SHORT_MIN, 1, -4, 31, JSONB.Constants.BC_INT64_SHORT_MIN, 127, -4, 127, -4, 0, 0};
    }

    private static byte[] get_N() {
        return new byte[]{127, -4, 127, -4, 30, 0, 7, -32, 0, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 127, -4, 127, -4, 0, 0};
    }

    private static byte[] get_O() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 63, -8, 96, 12, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 96, 12, 63, -8, 31, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_P() {
        return new byte[]{127, -4, 127, -4, 65, 0, 65, 0, 99, 0, 62, 0, 28, 0, 0, 0};
    }

    private static byte[] get_Q() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 63, -8, 96, 12, JSONB.Constants.BC_INT32_SHORT_MIN, 52, 96, 28, 63, -4, 31, -12, 0, 0};
    }

    private static byte[] get_R() {
        return new byte[]{127, -4, 127, -4, 65, 0, 65, 0, 65, JSONB.Constants.BC_INT64_SHORT_MIN, 99, 112, 62, 28, 0, 0};
    }

    private static byte[] get_S() {
        return new byte[]{28, JSONB.Constants.BC_INT32_BYTE_MIN, 62, JSONB.Constants.BC_INT32_BYTE_ZERO, 98, 12, 67, 4, 97, -116, JSONB.Constants.BC_INT32_BYTE_ZERO, -8, 24, 112, 0, 0};
    }

    private static byte[] get_T() {
        return new byte[]{JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 127, -4, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0, 0};
    }

    private static byte[] get_U() {
        return new byte[]{127, JSONB.Constants.BC_INT32_NUM_MIN, 127, -8, 0, 12, 0, 4, 0, 12, 127, -8, 127, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_V() {
        return new byte[]{112, 0, 127, 0, 15, -32, 0, -4, 15, -32, 127, 0, 112, 0, 0, 0};
    }

    private static byte[] get_W() {
        return new byte[]{127, ByteCompanionObject.MIN_VALUE, 15, JSONB.Constants.BC_INT32_NUM_MIN, 1, -4, 127, ByteCompanionObject.MIN_VALUE, 1, -4, 15, JSONB.Constants.BC_INT32_NUM_MIN, 127, ByteCompanionObject.MIN_VALUE, 0, 0};
    }

    private static byte[] get_X() {
        return new byte[]{96, 12, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 60, 30, JSONB.Constants.BC_INT32_NUM_MIN, 7, JSONB.Constants.BC_INT64_SHORT_MIN, 30, JSONB.Constants.BC_INT32_NUM_MIN, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 60, 96, 12, 0, 0};
    }

    private static byte[] get_Y() {
        return new byte[]{96, 0, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 0, 30, 0, 7, -4, 30, 0, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 0, 96, 0, 0, 0};
    }

    private static byte[] get_Z() {
        return new byte[]{JSONB.Constants.BC_INT32_SHORT_MIN, 12, JSONB.Constants.BC_INT32_SHORT_MIN, 60, JSONB.Constants.BC_INT32_SHORT_MIN, -12, 67, -124, 94, 4, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 4, 96, 4, 0, 0};
    }

    private static byte[] get_a() {
        return new byte[]{1, 24, 3, 60, 2, 100, 2, JSONB.Constants.BC_INT32_SHORT_ZERO, 3, -4, 1, -4, 0, 4, 0, 0};
    }

    private static byte[] get_b() {
        return new byte[]{127, -4, 127, -4, 3, 12, 2, 4, 3, 12, 1, -8, 0, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_c() {
        return new byte[]{0, JSONB.Constants.BC_INT32_NUM_MIN, 1, -8, 3, 12, 2, 4, 2, 4, 3, 12, 1, 8, 0, 0};
    }

    private static byte[] get_d() {
        return new byte[]{0, JSONB.Constants.BC_INT32_NUM_MIN, 1, -8, 3, 12, 2, 4, 3, 12, 127, -4, 127, -4, 0, 0};
    }

    private static byte[] get_e() {
        return new byte[]{0, JSONB.Constants.BC_INT32_NUM_MIN, 1, -8, 3, 76, 2, JSONB.Constants.BC_INT32_SHORT_ZERO, 3, JSONB.Constants.BC_INT32_SHORT_ZERO, 1, -52, 0, JSONB.Constants.BC_INT64_BYTE_MIN, 0, 0};
    }

    private static byte[] get_f() {
        return new byte[]{2, 0, 2, 0, 63, -4, 127, -4, 66, 0, 66, 0, 0, 0, 0, 0};
    }

    private static byte[] get_g() {
        return new byte[]{0, 12, 1, JSONB.Constants.BC_INT64, 3, -14, 2, 82, 3, -46, 3, -98, 2, 12, 0, 0};
    }

    private static byte[] get_h() {
        return new byte[]{127, -4, 127, -4, 3, 0, 2, 0, 3, 0, 1, -4, 0, -4, 0, 0};
    }

    private static byte[] get_i() {
        return new byte[]{0, 0, 0, 0, 0, 0, 51, -4, 51, -4, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_j() {
        return new byte[]{0, 0, 0, 2, 0, 2, 51, -2, 51, -4, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_k() {
        return new byte[]{127, -4, 127, -4, 0, 96, 0, -32, 1, JSONB.Constants.BC_FALSE, 3, 24, 2, 12, 0, 0};
    }

    private static byte[] get_l() {
        return new byte[]{0, 0, 0, 0, 0, 0, 127, -4, 127, -4, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_m() {
        return new byte[]{3, -4, 3, -4, 3, 0, 3, -4, 2, 0, 3, -4, 1, -4, 0, 0};
    }

    private static byte[] get_n() {
        return new byte[]{3, -4, 3, -4, 3, 0, 2, 0, 3, 0, 1, -4, 0, -4, 0, 0};
    }

    private static byte[] get_o() {
        return new byte[]{0, JSONB.Constants.BC_INT32_NUM_MIN, 1, -8, 3, 12, 2, 4, 3, 12, 1, -8, 0, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0};
    }

    private static byte[] get_p() {
        return new byte[]{3, -2, 3, -2, 3, 24, 2, 8, 3, 24, 1, JSONB.Constants.BC_INT32_NUM_MIN, 0, -32, 0, 0};
    }

    private static byte[] get_q() {
        return new byte[]{0, -32, 1, JSONB.Constants.BC_INT32_NUM_MIN, 3, 24, 2, 8, 3, 24, 3, -2, 3, -2, 0, 0};
    }

    private static byte[] get_r() {
        return new byte[]{0, 0, 3, -4, 3, -4, 3, 0, 2, 0, 2, 0, 2, 0, 0, 0};
    }

    private static byte[] get_s() {
        return new byte[]{1, -120, 3, -52, 2, JSONB.Constants.BC_INT32_SHORT_ZERO, 2, 100, 2, 36, 3, 60, 1, 24, 0, 0};
    }

    private static byte[] get_t() {
        return new byte[]{2, 0, 2, 0, 63, -8, 63, -4, 2, 4, 2, 4, 0, 0, 0, 0};
    }

    private static byte[] get_u() {
        return new byte[]{3, JSONB.Constants.BC_INT32_NUM_MIN, 3, -8, 0, 12, 0, 4, 0, 12, 3, -4, 3, -4, 0, 0};
    }

    private static byte[] get_v() {
        return new byte[]{3, 0, 3, JSONB.Constants.BC_INT64_SHORT_MIN, 0, JSONB.Constants.BC_INT32_NUM_MIN, 0, 60, 0, JSONB.Constants.BC_INT32_NUM_MIN, 3, JSONB.Constants.BC_INT64_SHORT_MIN, 3, 0, 0, 0};
    }

    private static byte[] get_w() {
        return new byte[]{3, -32, 3, -4, 0, JSONB.Constants.BC_STR_UTF16LE, 3, -32, 0, JSONB.Constants.BC_STR_UTF16LE, 3, -4, 3, -32, 0, 0};
    }

    private static byte[] get_x() {
        return new byte[]{2, 4, 3, 12, 1, -104, 0, JSONB.Constants.BC_INT32_NUM_MIN, 1, -104, 3, 12, 2, 4, 0, 0};
    }

    private static byte[] get_y() {
        return new byte[]{3, 2, 3, -62, 0, -10, 0, 60, 0, JSONB.Constants.BC_INT32_NUM_MIN, 3, JSONB.Constants.BC_INT64_SHORT_MIN, 3, 0, 0, 0};
    }

    private static byte[] get_z() {
        return new byte[]{2, 12, 2, 28, 2, 52, 2, 100, 2, JSONB.Constants.BC_INT64_SHORT_ZERO, 3, -124, 3, 4, 0, 0};
    }

    private static byte[] get_1() {
        return new byte[]{0, 0, 32, 0, 32, 0, 127, -4, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_2() {
        return new byte[]{24, 12, 32, 20, JSONB.Constants.BC_INT32_SHORT_MIN, 100, JSONB.Constants.BC_INT32_SHORT_MIN, -124, 35, 4, 28, 4, 0, 0, 0, 0};
    }

    private static byte[] get_3() {
        return new byte[]{24, JSONB.Constants.BC_INT32_BYTE_MIN, 32, 8, 65, 4, 65, 4, 34, -120, 28, 112, 0, 0, 0, 0};
    }

    private static byte[] get_4() {
        return new byte[]{0, 96, 1, -96, 6, 32, 24, 32, 127, -4, 0, 32, 0, 0, 0, 0};
    }

    private static byte[] get_5() {
        return new byte[]{127, JSONB.Constants.BC_INT32_NUM_16, 66, 8, JSONB.Constants.BC_INT32_SHORT_ZERO, 4, JSONB.Constants.BC_INT32_SHORT_ZERO, 4, 66, 8, 65, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0};
    }

    private static byte[] get_6() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 33, 8, 66, 4, 66, 4, 33, 8, 24, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0};
    }

    private static byte[] get_7() {
        return new byte[]{JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 60, 65, JSONB.Constants.BC_INT64_SHORT_MIN, JSONB.Constants.BC_STR_ASCII_FIX_5, 0, 112, 0, 0, 0, 0, 0};
    }

    private static byte[] get_8() {
        return new byte[]{28, 112, 34, -120, 65, 4, 65, 4, 34, -120, 28, 112, 0, 0, 0, 0};
    }

    private static byte[] get_9() {
        return new byte[]{30, JSONB.Constants.BC_INT32_BYTE_MIN, 33, 8, JSONB.Constants.BC_INT32_SHORT_MIN, -124, JSONB.Constants.BC_INT32_SHORT_MIN, -124, 33, 8, 31, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0};
    }

    private static byte[] get_0() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 32, 8, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, 32, 8, 31, JSONB.Constants.BC_INT32_NUM_MIN, 0, 0, 0, 0};
    }

    private static byte[] get_question() {
        return new byte[]{24, 0, JSONB.Constants.BC_INT32_BYTE_ZERO, 0, 96, -52, 65, -52, 99, 12, 62, 0, 28, 0, 0, 0};
    }

    private static byte[] get_xiaolaoshu() {
        return new byte[]{31, JSONB.Constants.BC_INT32_NUM_MIN, 63, -8, 103, -52, 79, -28, 111, -28, 63, -20, 31, JSONB.Constants.BC_INT64_BYTE_MIN, 0, 0};
    }

    private static byte[] get_jinghao() {
        return new byte[]{4, 32, 127, -2, 127, -2, 4, 32, 127, -2, 127, -2, 4, 32, 0, 0};
    }

    private static byte[] get_meiyuanfuhao() {
        return new byte[]{24, JSONB.Constants.BC_INT32_BYTE_MIN, 60, JSONB.Constants.BC_INT32_BYTE_ZERO, 102, 12, -1, -2, 97, -116, JSONB.Constants.BC_INT32_BYTE_ZERO, -8, 24, 112, 0, 0};
    }

    private static byte[] get_baifenbi() {
        return new byte[]{JSONB.Constants.BC_INT32_BYTE_ZERO, 12, JSONB.Constants.BC_STR_UTF16LE, 60, JSONB.Constants.BC_INT32_SHORT_ZERO, JSONB.Constants.BC_INT32_NUM_MIN, 127, -4, 30, JSONB.Constants.BC_INT32_SHORT_ZERO, JSONB.Constants.BC_STR_ASCII_FIX_MAX, JSONB.Constants.BC_STR_UTF16LE, 96, JSONB.Constants.BC_INT32_BYTE_ZERO, 0, 0};
    }

    private static byte[] get_yufuhao() {
        return new byte[]{0, JSONB.Constants.BC_STR_ASCII_FIX_MAX, 60, -4, 127, -124, 67, -52, JSONB.Constants.BC_STR_GB18030, -8, 60, -8, 0, -52, 0, 0};
    }

    private static byte[] get_xinghao() {
        return new byte[]{0, 0, 12, 96, 6, JSONB.Constants.BC_INT64_SHORT_MIN, 31, JSONB.Constants.BC_INT32_NUM_MIN, 31, JSONB.Constants.BC_INT32_NUM_MIN, 6, JSONB.Constants.BC_INT64_SHORT_MIN, 12, 96, 0, 0};
    }

    private static byte[] get_xiahuaxian() {
        return new byte[]{0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0};
    }

    private static byte[] get_shuangsuoxiehao() {
        return new byte[]{32, 0, -32, 0, JSONB.Constants.BC_INT64_SHORT_MIN, 0, -96, 0, -32, 0, JSONB.Constants.BC_INT64_SHORT_MIN, 0, ByteCompanionObject.MIN_VALUE, 0, 0, 0};
    }

    private static byte[] get_xiegang() {
        return new byte[]{0, 12, 0, 60, 0, JSONB.Constants.BC_INT32_NUM_MIN, 1, JSONB.Constants.BC_INT64_SHORT_MIN, 7, 0, 30, 0, JSONB.Constants.BC_INT32_BYTE_ZERO, 0, 0, 0};
    }

    private static byte[] get_add() {
        return new byte[]{0, 0, 1, 0, 1, 0, 15, -32, 15, -32, 1, 0, 1, 0, 0, 0};
    }

    private static byte[] get_pozhehao() {
        return new byte[]{JSONB.Constants.BC_INT32_SHORT_MIN, 0, ByteCompanionObject.MIN_VALUE, 0, ByteCompanionObject.MIN_VALUE, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, ByteCompanionObject.MIN_VALUE, 0, 0, 0, 0, 0};
    }

    private static byte[] get_exclamation() {
        return new byte[]{0, 0, 0, 0, 0, 0, 127, -52, 0, 12, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_yunsuanfuhao() {
        return new byte[]{0, 0, 32, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, ByteCompanionObject.MIN_VALUE, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 32, 0, 0, 0, 0, 0};
    }

    private static byte[] get_zuokuohao() {
        return new byte[]{0, 0, 0, 0, 15, -32, JSONB.Constants.BC_INT32_BYTE_MIN, 24, JSONB.Constants.BC_INT32_SHORT_MIN, 4, ByteCompanionObject.MIN_VALUE, 2, 0, 0, 0, 0};
    }

    private static byte[] get_youkuohao() {
        return new byte[]{0, 0, ByteCompanionObject.MIN_VALUE, 2, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_BYTE_MIN, 24, 15, -32, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_piedian() {
        return new byte[]{0, 0, 0, 0, ByteCompanionObject.MIN_VALUE, 0, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 32, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_dash() {
        return new byte[]{0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0};
    }

    private static byte[] get_equal() {
        return new byte[]{0, 0, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 0, 0, 0, 0};
    }

    private static byte[] get_zuozhongkuohao() {
        return new byte[]{0, 0, -1, -2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, 0, 0, 0, 0};
    }

    private static byte[] get_youzhongkuohao() {
        return new byte[]{0, 0, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, -1, -2, 0, 0, 0, 0};
    }

    private static byte[] get_zuodakuohao() {
        return new byte[]{0, 0, 0, 0, 1, 0, -2, -2, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, 0, 0, 0, 0};
    }

    private static byte[] get_youdakuohao() {
        return new byte[]{0, 0, ByteCompanionObject.MIN_VALUE, 2, ByteCompanionObject.MIN_VALUE, 2, -2, -2, 1, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_fenhao() {
        return new byte[]{0, 0, 0, 0, 6, 26, 6, 28, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_suoxiehao() {
        return new byte[]{0, 0, 0, 0, JSONB.Constants.BC_INT64_BYTE_ZERO, 0, -32, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_fanxiegang() {
        return new byte[]{96, 0, 25, 32, 7, 32, 1, -4, 7, 32, 25, 32, 96, 0, 0, 0};
    }

    private static byte[] get_maohao() {
        return new byte[]{0, 0, 0, 0, 6, 24, 6, 24, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_shuxian() {
        return new byte[]{0, 0, 0, 0, 0, 0, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_comma() {
        return new byte[]{0, 0, 0, 0, 0, 26, 0, 28, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_period() {
        return new byte[]{0, 0, 0, 0, 0, 12, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    private static byte[] get_left() {
        return new byte[]{1, 0, 2, ByteCompanionObject.MIN_VALUE, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 8, 32, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 32, 8, 0, 0, 0, 0};
    }

    private static byte[] get_right() {
        return new byte[]{32, 8, JSONB.Constants.BC_INT32_NUM_16, JSONB.Constants.BC_INT32_NUM_16, 8, 32, 4, JSONB.Constants.BC_INT32_SHORT_MIN, 2, ByteCompanionObject.MIN_VALUE, 1, 0, 0, 0, 0, 0};
    }

    private static byte[] get_space() {
        return new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }

    public static byte[] getSpace2() {
        return new byte[]{0, 0, 0, 0};
    }
}
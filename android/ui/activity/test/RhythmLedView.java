package cn.com.heaton.shiningmask.ui.activity.test;

import android.content.Context;
import android.graphics.Color;
import android.os.Handler;
import android.os.Message;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.base.app.MyTimeTask;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.widget.LedItemRhyView;
import cn.com.heaton.shiningmask.ui.widget.holocolorpicker.OpacityBar;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import kotlin.UByte;
import org.apache.http.HttpStatus;

/* JADX INFO: loaded from: classes.dex */
public class RhythmLedView extends ViewGroup {
    public static final int MODE_ERASER = 2;
    public static final int MODE_NO = 0;
    public static final int MODE_PAINT = 1;
    public static final int ORIENTATION_LANDSCAPE = 1;
    public static final int ORIENTATION_PORTRAIT = 0;
    private static final String TAG = "RhythmLedView";
    int[][] animData;
    private boolean animEnable;
    private boolean flag;
    final Handler handler;
    private int heightCount;
    int heightSize;
    int index;
    private int mode;
    int moveMax;
    int moveYMax;
    private int offset;
    private int orientation;
    private int pointAllLength;
    private int pointLength;
    private int pointMargin;
    private int pointYAllLength;
    private int rhy1Color_1;
    private int rhy1Color_10;
    private int rhy1Color_11;
    private int rhy1Color_12;
    private int rhy1Color_13;
    private int rhy1Color_14;
    private int rhy1Color_15;
    private int rhy1Color_16;
    private int rhy1Color_17;
    private int rhy1Color_18;
    private int rhy1Color_19;
    private int rhy1Color_2;
    private int rhy1Color_20;
    private int rhy1Color_21;
    private int rhy1Color_22;
    private int rhy1Color_23;
    private int rhy1Color_24;
    private int rhy1Color_3;
    private int rhy1Color_4;
    private int rhy1Color_5;
    private int rhy1Color_6;
    private int rhy1Color_7;
    private int rhy1Color_8;
    private int rhy1Color_9;
    private int rhy2Color_1;
    private int rhy2Color_10;
    private int rhy2Color_11;
    private int rhy2Color_12;
    private int rhy2Color_2;
    private int rhy2Color_3;
    private int rhy2Color_4;
    private int rhy2Color_5;
    private int rhy2Color_6;
    private int rhy2Color_7;
    private int rhy2Color_8;
    private int rhy2Color_9;
    private int rhy5Color_1;
    private int rhy5Color_10;
    private int rhy5Color_11;
    private int rhy5Color_12;
    private int rhy5Color_2;
    private int rhy5Color_3;
    private int rhy5Color_4;
    private int rhy5Color_5;
    private int rhy5Color_6;
    private int rhy5Color_7;
    private int rhy5Color_8;
    private int rhy5Color_9;
    private int selectedColor1;
    private int selectedColor2;
    private int selectedColor3;
    private int selectedColor4;
    private int selectedColorGray1;
    private int selectedColorGray2;
    private int selectedColorGray3;
    private MyTimeTask task;
    private byte[] textData;
    private Timer timer;
    private int unSelectedColor;
    private int widthCount;
    int widthSize;
    private int xMore;
    private int yMore;

    public RhythmLedView(Context context) {
        super(context);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.rgb(17, HttpStatus.SC_NON_AUTHORITATIVE_INFORMATION, 231);
        this.selectedColor4 = Color.rgb(255, 255, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = 0;
        this.rhy1Color_1 = Color.rgb(255, 8, 0);
        this.rhy1Color_2 = Color.rgb(255, 47, 0);
        this.rhy1Color_3 = Color.rgb(255, 210, 0);
        this.rhy1Color_4 = Color.rgb(255, 157, 0);
        this.rhy1Color_5 = Color.rgb(241, 255, 0);
        this.rhy1Color_6 = Color.rgb(216, 255, 0);
        this.rhy1Color_7 = Color.rgb(150, 255, 0);
        this.rhy1Color_8 = Color.rgb(101, 255, 0);
        this.rhy1Color_9 = Color.rgb(0, 255, 1);
        this.rhy1Color_10 = Color.rgb(0, 255, 44);
        this.rhy1Color_11 = Color.rgb(0, 255, Opcodes.IF_ICMPNE);
        this.rhy1Color_12 = Color.rgb(0, 255, HttpStatus.SC_CREATED);
        this.rhy1Color_13 = Color.rgb(0, 254, 255);
        this.rhy1Color_14 = Color.rgb(3, 238, 255);
        this.rhy1Color_15 = Color.rgb(9, Opcodes.ARRAYLENGTH, 249);
        this.rhy1Color_16 = Color.rgb(14, Opcodes.IF_ICMPLT, 235);
        this.rhy1Color_17 = Color.rgb(33, 109, 211);
        this.rhy1Color_18 = Color.rgb(53, 86, OpacityBar.FULL_OPACITY);
        this.rhy1Color_19 = Color.rgb(156, 35, 245);
        this.rhy1Color_20 = Color.rgb(Opcodes.GETSTATIC, 27, 253);
        this.rhy1Color_21 = Color.rgb(219, 13, 255);
        this.rhy1Color_22 = Color.rgb(236, 7, 255);
        this.rhy1Color_23 = Color.rgb(255, 0, 246);
        this.rhy1Color_24 = Color.rgb(255, 0, 213);
        this.rhy2Color_1 = Color.rgb(255, 0, 123);
        this.rhy2Color_2 = Color.rgb(255, 0, 249);
        this.rhy2Color_3 = Color.rgb(215, 15, 255);
        this.rhy2Color_4 = Color.rgb(143, 40, 240);
        this.rhy2Color_5 = Color.rgb(29, 117, 214);
        this.rhy2Color_6 = Color.rgb(7, HttpStatus.SC_CREATED, 255);
        this.rhy2Color_7 = Color.rgb(0, 255, 247);
        this.rhy2Color_8 = Color.rgb(0, 255, 134);
        this.rhy2Color_9 = Color.rgb(29, 255, 0);
        this.rhy2Color_10 = Color.rgb(HttpStatus.SC_NO_CONTENT, 255, 0);
        this.rhy2Color_11 = Color.rgb(255, 211, 0);
        this.rhy2Color_12 = Color.rgb(255, 55, 0);
        this.rhy5Color_1 = Color.rgb(255, 255, 0);
        this.rhy5Color_2 = Color.rgb(255, 248, 0);
        this.rhy5Color_3 = Color.rgb(255, 238, 0);
        this.rhy5Color_4 = Color.rgb(255, 226, 0);
        this.rhy5Color_5 = Color.rgb(255, 212, 1);
        this.rhy5Color_6 = Color.rgb(255, Opcodes.IFNULL, 1);
        this.rhy5Color_7 = Color.rgb(255, Opcodes.INVOKESPECIAL, 1);
        this.rhy5Color_8 = Color.rgb(255, Opcodes.GOTO, 1);
        this.rhy5Color_9 = Color.rgb(255, 153, 1);
        this.rhy5Color_10 = Color.rgb(255, 139, 2);
        this.rhy5Color_11 = Color.rgb(255, 127, 2);
        this.rhy5Color_12 = Color.rgb(255, 117, 2);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmLedView.2
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1001) {
                    RhythmLedView rhythmLedView = RhythmLedView.this;
                    rhythmLedView.showImage(rhythmLedView.animData);
                }
                super.handleMessage(message);
            }
        };
    }

    public RhythmLedView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.rgb(17, HttpStatus.SC_NON_AUTHORITATIVE_INFORMATION, 231);
        this.selectedColor4 = Color.rgb(255, 255, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = 0;
        this.rhy1Color_1 = Color.rgb(255, 8, 0);
        this.rhy1Color_2 = Color.rgb(255, 47, 0);
        this.rhy1Color_3 = Color.rgb(255, 210, 0);
        this.rhy1Color_4 = Color.rgb(255, 157, 0);
        this.rhy1Color_5 = Color.rgb(241, 255, 0);
        this.rhy1Color_6 = Color.rgb(216, 255, 0);
        this.rhy1Color_7 = Color.rgb(150, 255, 0);
        this.rhy1Color_8 = Color.rgb(101, 255, 0);
        this.rhy1Color_9 = Color.rgb(0, 255, 1);
        this.rhy1Color_10 = Color.rgb(0, 255, 44);
        this.rhy1Color_11 = Color.rgb(0, 255, Opcodes.IF_ICMPNE);
        this.rhy1Color_12 = Color.rgb(0, 255, HttpStatus.SC_CREATED);
        this.rhy1Color_13 = Color.rgb(0, 254, 255);
        this.rhy1Color_14 = Color.rgb(3, 238, 255);
        this.rhy1Color_15 = Color.rgb(9, Opcodes.ARRAYLENGTH, 249);
        this.rhy1Color_16 = Color.rgb(14, Opcodes.IF_ICMPLT, 235);
        this.rhy1Color_17 = Color.rgb(33, 109, 211);
        this.rhy1Color_18 = Color.rgb(53, 86, OpacityBar.FULL_OPACITY);
        this.rhy1Color_19 = Color.rgb(156, 35, 245);
        this.rhy1Color_20 = Color.rgb(Opcodes.GETSTATIC, 27, 253);
        this.rhy1Color_21 = Color.rgb(219, 13, 255);
        this.rhy1Color_22 = Color.rgb(236, 7, 255);
        this.rhy1Color_23 = Color.rgb(255, 0, 246);
        this.rhy1Color_24 = Color.rgb(255, 0, 213);
        this.rhy2Color_1 = Color.rgb(255, 0, 123);
        this.rhy2Color_2 = Color.rgb(255, 0, 249);
        this.rhy2Color_3 = Color.rgb(215, 15, 255);
        this.rhy2Color_4 = Color.rgb(143, 40, 240);
        this.rhy2Color_5 = Color.rgb(29, 117, 214);
        this.rhy2Color_6 = Color.rgb(7, HttpStatus.SC_CREATED, 255);
        this.rhy2Color_7 = Color.rgb(0, 255, 247);
        this.rhy2Color_8 = Color.rgb(0, 255, 134);
        this.rhy2Color_9 = Color.rgb(29, 255, 0);
        this.rhy2Color_10 = Color.rgb(HttpStatus.SC_NO_CONTENT, 255, 0);
        this.rhy2Color_11 = Color.rgb(255, 211, 0);
        this.rhy2Color_12 = Color.rgb(255, 55, 0);
        this.rhy5Color_1 = Color.rgb(255, 255, 0);
        this.rhy5Color_2 = Color.rgb(255, 248, 0);
        this.rhy5Color_3 = Color.rgb(255, 238, 0);
        this.rhy5Color_4 = Color.rgb(255, 226, 0);
        this.rhy5Color_5 = Color.rgb(255, 212, 1);
        this.rhy5Color_6 = Color.rgb(255, Opcodes.IFNULL, 1);
        this.rhy5Color_7 = Color.rgb(255, Opcodes.INVOKESPECIAL, 1);
        this.rhy5Color_8 = Color.rgb(255, Opcodes.GOTO, 1);
        this.rhy5Color_9 = Color.rgb(255, 153, 1);
        this.rhy5Color_10 = Color.rgb(255, 139, 2);
        this.rhy5Color_11 = Color.rgb(255, 127, 2);
        this.rhy5Color_12 = Color.rgb(255, 117, 2);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmLedView.2
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1001) {
                    RhythmLedView rhythmLedView = RhythmLedView.this;
                    rhythmLedView.showImage(rhythmLedView.animData);
                }
                super.handleMessage(message);
            }
        };
    }

    public RhythmLedView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.widthCount = 5;
        this.heightCount = 10;
        this.pointMargin = 1;
        this.selectedColor1 = Color.argb(89, 82, 227, 255);
        this.selectedColor2 = Color.argb(Opcodes.IF_ACMPNE, 82, 227, 255);
        this.selectedColor3 = Color.rgb(17, HttpStatus.SC_NON_AUTHORITATIVE_INFORMATION, 231);
        this.selectedColor4 = Color.rgb(255, 255, 255);
        this.selectedColorGray1 = Color.argb(89, 42, 122, 142);
        this.selectedColorGray2 = Color.argb(Opcodes.IF_ACMPNE, 42, 122, 142);
        this.selectedColorGray3 = Color.rgb(42, 122, 142);
        this.unSelectedColor = 0;
        this.rhy1Color_1 = Color.rgb(255, 8, 0);
        this.rhy1Color_2 = Color.rgb(255, 47, 0);
        this.rhy1Color_3 = Color.rgb(255, 210, 0);
        this.rhy1Color_4 = Color.rgb(255, 157, 0);
        this.rhy1Color_5 = Color.rgb(241, 255, 0);
        this.rhy1Color_6 = Color.rgb(216, 255, 0);
        this.rhy1Color_7 = Color.rgb(150, 255, 0);
        this.rhy1Color_8 = Color.rgb(101, 255, 0);
        this.rhy1Color_9 = Color.rgb(0, 255, 1);
        this.rhy1Color_10 = Color.rgb(0, 255, 44);
        this.rhy1Color_11 = Color.rgb(0, 255, Opcodes.IF_ICMPNE);
        this.rhy1Color_12 = Color.rgb(0, 255, HttpStatus.SC_CREATED);
        this.rhy1Color_13 = Color.rgb(0, 254, 255);
        this.rhy1Color_14 = Color.rgb(3, 238, 255);
        this.rhy1Color_15 = Color.rgb(9, Opcodes.ARRAYLENGTH, 249);
        this.rhy1Color_16 = Color.rgb(14, Opcodes.IF_ICMPLT, 235);
        this.rhy1Color_17 = Color.rgb(33, 109, 211);
        this.rhy1Color_18 = Color.rgb(53, 86, OpacityBar.FULL_OPACITY);
        this.rhy1Color_19 = Color.rgb(156, 35, 245);
        this.rhy1Color_20 = Color.rgb(Opcodes.GETSTATIC, 27, 253);
        this.rhy1Color_21 = Color.rgb(219, 13, 255);
        this.rhy1Color_22 = Color.rgb(236, 7, 255);
        this.rhy1Color_23 = Color.rgb(255, 0, 246);
        this.rhy1Color_24 = Color.rgb(255, 0, 213);
        this.rhy2Color_1 = Color.rgb(255, 0, 123);
        this.rhy2Color_2 = Color.rgb(255, 0, 249);
        this.rhy2Color_3 = Color.rgb(215, 15, 255);
        this.rhy2Color_4 = Color.rgb(143, 40, 240);
        this.rhy2Color_5 = Color.rgb(29, 117, 214);
        this.rhy2Color_6 = Color.rgb(7, HttpStatus.SC_CREATED, 255);
        this.rhy2Color_7 = Color.rgb(0, 255, 247);
        this.rhy2Color_8 = Color.rgb(0, 255, 134);
        this.rhy2Color_9 = Color.rgb(29, 255, 0);
        this.rhy2Color_10 = Color.rgb(HttpStatus.SC_NO_CONTENT, 255, 0);
        this.rhy2Color_11 = Color.rgb(255, 211, 0);
        this.rhy2Color_12 = Color.rgb(255, 55, 0);
        this.rhy5Color_1 = Color.rgb(255, 255, 0);
        this.rhy5Color_2 = Color.rgb(255, 248, 0);
        this.rhy5Color_3 = Color.rgb(255, 238, 0);
        this.rhy5Color_4 = Color.rgb(255, 226, 0);
        this.rhy5Color_5 = Color.rgb(255, 212, 1);
        this.rhy5Color_6 = Color.rgb(255, Opcodes.IFNULL, 1);
        this.rhy5Color_7 = Color.rgb(255, Opcodes.INVOKESPECIAL, 1);
        this.rhy5Color_8 = Color.rgb(255, Opcodes.GOTO, 1);
        this.rhy5Color_9 = Color.rgb(255, 153, 1);
        this.rhy5Color_10 = Color.rgb(255, 139, 2);
        this.rhy5Color_11 = Color.rgb(255, 127, 2);
        this.rhy5Color_12 = Color.rgb(255, 117, 2);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmLedView.2
            @Override // android.os.Handler
            public void handleMessage(Message message) {
                if (message.what == 1001) {
                    RhythmLedView rhythmLedView = RhythmLedView.this;
                    rhythmLedView.showImage(rhythmLedView.animData);
                }
                super.handleMessage(message);
            }
        };
    }

    public void init(int i, int i2) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemRhyView ledItemRhyView = new LedItemRhyView(getContext());
            ledItemRhyView.setViewNumber(i3);
            ledItemRhyView.setColumnNumber(i3 / i2);
            ledItemRhyView.setRowNumber(i3 % i2);
            ledItemRhyView.setPaint(this.unSelectedColor);
            ledItemRhyView.postInvalidate();
            addView(ledItemRhyView);
        }
    }

    public void init(int i, int i2, float f) {
        this.widthCount = i;
        this.heightCount = i2;
        for (int i3 = 0; i3 < i * i2; i3++) {
            LedItemRhyView ledItemRhyView = new LedItemRhyView(getContext());
            ledItemRhyView.setViewNumber(i3);
            ledItemRhyView.setColumnNumber(i3 / i2);
            ledItemRhyView.setRowNumber(i3 % i2);
            ledItemRhyView.setPaint(this.unSelectedColor, f);
            ledItemRhyView.postInvalidate();
            addView(ledItemRhyView);
        }
    }

    public void removeAllChildView() {
        int childCount = getChildCount();
        if (childCount > 0) {
            for (int i = 0; i < childCount; i++) {
                removeViewAt(0);
            }
        }
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        measureChildren(i, i2);
        this.widthSize = View.MeasureSpec.getSize(i);
        int size = View.MeasureSpec.getSize(i2);
        this.heightSize = size;
        int i3 = this.widthSize;
        int i4 = this.widthCount;
        int i5 = i3 % i4;
        this.xMore = i5;
        int i6 = this.heightCount;
        this.yMore = size % i6;
        int i7 = i3 / i4;
        this.pointAllLength = i7;
        int i8 = size / i6;
        this.pointYAllLength = i8;
        int i9 = this.pointMargin;
        this.pointLength = i7 - (i9 * 2);
        int i10 = i5 / 2;
        this.offset = i10;
        this.moveMax = (i7 * i6) - size;
        this.moveYMax = (i8 * i4) - i3;
        int i11 = (i7 * i6) - (i9 + i10);
        this.heightSize = i11;
        setMeasuredDimension(i3, i11);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        int childCount = getChildCount();
        for (int i5 = 0; i5 < childCount; i5++) {
            View childAt = getChildAt(i5);
            int i6 = this.heightCount;
            int i7 = this.pointAllLength;
            int i8 = this.pointMargin;
            int i9 = ((i5 / i6) * i7) + i8 + this.offset;
            int i10 = ((i5 % i6) * i7) + i8;
            int i11 = this.pointLength;
            childAt.layout(i9, i10, i9 + i11, i11 + i10);
        }
    }

    public void clearSelected() {
        for (int i = 0; i < getChildCount(); i++) {
            LedItemRhyView ledItemRhyView = (LedItemRhyView) getChildAt(i);
            ledItemRhyView.setChecked(false);
            ledItemRhyView.setPaint(this.unSelectedColor);
            ledItemRhyView.postInvalidate();
        }
    }

    public void setData(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        int childCount = getChildCount();
        for (int i = 0; i < childCount; i++) {
            LedItemRhyView ledItemRhyView = (LedItemRhyView) getChildAt(i);
            if (bArr[i] == 1) {
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.selectedColor3);
            } else {
                ledItemRhyView.setChecked(false);
                ledItemRhyView.setPaint(this.unSelectedColor);
            }
            ledItemRhyView.postInvalidate();
        }
    }

    public void setData(List<Integer> list) {
        if (list == null) {
            return;
        }
        getChildCount();
        for (int i = 0; i < list.size(); i++) {
            LedItemRhyView ledItemRhyView = (LedItemRhyView) getChildAt(i);
            LogUtil.d("====data:" + list.get(i));
            if (list.get(i).intValue() == 1) {
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.selectedColor3);
            } else {
                ledItemRhyView.setChecked(false);
                ledItemRhyView.setPaint(this.unSelectedColor);
            }
            ledItemRhyView.postInvalidate();
        }
    }

    public void setTextData(byte[] bArr) {
        this.textData = bArr;
        clearSelected();
        int length = bArr.length / 2;
        int[] iArr = new int[length];
        for (int i = 0; i < bArr.length / 2; i++) {
            int i2 = i * 2;
            iArr[i] = ((bArr[i2] & UByte.MAX_VALUE) * 64) + ((bArr[i2 + 1] & UByte.MAX_VALUE) >> 2);
        }
        for (int i3 = 0; i3 < 24; i3++) {
            if (i3 < length) {
                for (int i4 = 0; i4 < 9; i4++) {
                    int i5 = (iArr[i3] >> (i4 + 5)) & 1;
                    LedItemRhyView ledItemRhyView = getLedItemRhyView(i3, i4);
                    if (i5 == 1) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColor3);
                    } else {
                        ledItemRhyView.setChecked(false);
                        ledItemRhyView.setPaint(this.unSelectedColor);
                    }
                    ledItemRhyView.postInvalidate();
                }
            }
        }
    }

    private LedItemRhyView getLedItemRhyView(int i, int i2) {
        return (LedItemRhyView) getChildAt((i * 12) + (12 - (i2 + 1)));
    }

    public void setRhyData1(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        this.textData = bArr;
        clearSelected();
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 12) {
                arrayList.add(Integer.valueOf(height4));
                arrayList.add(Integer.valueOf(low4));
            }
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            int iIntValue = ((Integer) arrayList.get(i2)).intValue();
            for (int i3 = 0; i3 < iIntValue; i3++) {
                int i4 = i2 * 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4 - 2, i3);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4 - 3, i3);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                switch (i2) {
                    case 1:
                        ledItemRhyView.setPaint(this.rhy1Color_1);
                        ledItemRhyView2.setPaint(this.rhy1Color_2);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy1Color_3);
                        ledItemRhyView2.setPaint(this.rhy1Color_4);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy1Color_5);
                        ledItemRhyView2.setPaint(this.rhy1Color_6);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy1Color_7);
                        ledItemRhyView2.setPaint(this.rhy1Color_8);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy1Color_9);
                        ledItemRhyView2.setPaint(this.rhy1Color_10);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy1Color_11);
                        ledItemRhyView2.setPaint(this.rhy1Color_12);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy1Color_13);
                        ledItemRhyView2.setPaint(this.rhy1Color_14);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy1Color_15);
                        ledItemRhyView2.setPaint(this.rhy1Color_16);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy1Color_17);
                        ledItemRhyView2.setPaint(this.rhy1Color_18);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy1Color_19);
                        ledItemRhyView2.setPaint(this.rhy1Color_20);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy1Color_21);
                        ledItemRhyView2.setPaint(this.rhy1Color_22);
                        break;
                    case 12:
                        ledItemRhyView.setPaint(this.rhy1Color_23);
                        ledItemRhyView2.setPaint(this.rhy1Color_24);
                        break;
                }
            }
        }
    }

    public void setRhyData2(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        this.textData = bArr;
        clearSelected();
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 12) {
                arrayList.add(Integer.valueOf(height4));
                arrayList.add(Integer.valueOf(low4));
            }
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            int iIntValue = ((Integer) arrayList.get(i2)).intValue();
            for (int i3 = 0; i3 < iIntValue; i3++) {
                int i4 = i2 * 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4 - 2, i3);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4 - 3, i3);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                switch (i3) {
                    case 0:
                        ledItemRhyView.setPaint(this.rhy2Color_12);
                        ledItemRhyView2.setPaint(this.rhy2Color_12);
                        break;
                    case 1:
                        ledItemRhyView.setPaint(this.rhy2Color_11);
                        ledItemRhyView2.setPaint(this.rhy2Color_11);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy2Color_10);
                        ledItemRhyView2.setPaint(this.rhy2Color_10);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy2Color_9);
                        ledItemRhyView2.setPaint(this.rhy2Color_9);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy2Color_8);
                        ledItemRhyView2.setPaint(this.rhy2Color_8);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy2Color_7);
                        ledItemRhyView2.setPaint(this.rhy2Color_7);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy2Color_6);
                        ledItemRhyView2.setPaint(this.rhy2Color_6);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy2Color_5);
                        ledItemRhyView2.setPaint(this.rhy2Color_5);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy2Color_4);
                        ledItemRhyView2.setPaint(this.rhy2Color_4);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy2Color_3);
                        ledItemRhyView2.setPaint(this.rhy2Color_3);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy2Color_2);
                        ledItemRhyView2.setPaint(this.rhy2Color_2);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy2Color_1);
                        ledItemRhyView2.setPaint(this.rhy2Color_1);
                        break;
                }
            }
        }
    }

    public void setRhyData3(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        this.textData = bArr;
        clearSelected();
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 12) {
                arrayList.add(Integer.valueOf(height4));
                arrayList.add(Integer.valueOf(low4));
            }
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            int iIntValue = ((Integer) arrayList.get(i2)).intValue();
            for (int i3 = 0; i3 < iIntValue; i3++) {
                int i4 = i2 * 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4 - 2, i3);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4 - 3, i3);
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.selectedColor3);
                ledItemRhyView2.setChecked(true);
                ledItemRhyView2.setPaint(this.selectedColor3);
            }
        }
    }

    public void setRhyData4(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        this.textData = bArr;
        clearSelected();
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 12) {
                arrayList.add(Integer.valueOf(height4));
                arrayList.add(Integer.valueOf(low4));
            }
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            int iIntValue = ((Integer) arrayList.get(i2)).intValue();
            for (int i3 = 0; i3 < iIntValue; i3++) {
                int i4 = i2 * 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4 - 2, i3);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4 - 3, i3);
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.selectedColor4);
                ledItemRhyView2.setChecked(true);
                ledItemRhyView2.setPaint(this.selectedColor4);
            }
        }
    }

    public void setRhyData5(byte[] bArr) {
        if (bArr == null) {
            return;
        }
        this.textData = bArr;
        clearSelected();
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 12) {
                arrayList.add(Integer.valueOf(height4));
                arrayList.add(Integer.valueOf(low4));
            }
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            int iIntValue = ((Integer) arrayList.get(i2)).intValue();
            for (int i3 = 0; i3 < iIntValue; i3++) {
                int i4 = i2 * 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4 - 2, i3);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4 - 3, i3);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                switch (i3) {
                    case 0:
                        ledItemRhyView.setPaint(this.rhy5Color_12);
                        ledItemRhyView2.setPaint(this.rhy5Color_12);
                        break;
                    case 1:
                        ledItemRhyView.setPaint(this.rhy5Color_11);
                        ledItemRhyView2.setPaint(this.rhy5Color_11);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy5Color_10);
                        ledItemRhyView2.setPaint(this.rhy5Color_10);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy5Color_9);
                        ledItemRhyView2.setPaint(this.rhy5Color_9);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy5Color_8);
                        ledItemRhyView2.setPaint(this.rhy5Color_8);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy5Color_7);
                        ledItemRhyView2.setPaint(this.rhy5Color_7);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy5Color_6);
                        ledItemRhyView2.setPaint(this.rhy5Color_6);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy5Color_5);
                        ledItemRhyView2.setPaint(this.rhy5Color_5);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy5Color_4);
                        ledItemRhyView2.setPaint(this.rhy5Color_4);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy5Color_3);
                        ledItemRhyView2.setPaint(this.rhy5Color_3);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy5Color_2);
                        ledItemRhyView2.setPaint(this.rhy5Color_2);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy5Color_1);
                        ledItemRhyView2.setPaint(this.rhy5Color_1);
                        break;
                }
            }
        }
    }

    public void setImageData(int[][] iArr) {
        this.animEnable = true;
        this.animData = iArr;
        clearSelected();
    }

    private void showImage(int[][] iArr, int i) {
        if (i >= iArr.length) {
            return;
        }
        int[] iArr2 = iArr[i];
        for (int i2 = 0; i2 < iArr2.length; i2++) {
            for (int i3 = 0; i3 < 9; i3++) {
                int i4 = (iArr2[i2] >> (i3 * 2)) & 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i2, i3);
                if (ledItemRhyView != null) {
                    if (i4 == 1) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray1);
                    } else if (i4 == 2) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray2);
                    } else if (i4 == 3) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray3);
                    } else {
                        ledItemRhyView.setChecked(false);
                        ledItemRhyView.setPaint(this.unSelectedColor);
                    }
                    ledItemRhyView.postInvalidate();
                }
            }
        }
        this.index++;
    }

    public void setPointMargin(int i) {
        this.pointMargin = i;
    }

    public void setSelectedColor(int i) {
        this.selectedColor3 = i;
    }

    public void setMode(int i) {
        this.mode = i;
    }

    public int getOrientation() {
        return this.orientation;
    }

    public void setOrientation(int i) {
        this.orientation = i;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showImage(int[][] iArr) {
        if (this.index >= iArr.length) {
            this.index = 0;
        }
        int[] iArr2 = iArr[this.index];
        for (int i = 0; i < iArr2.length; i++) {
            for (int i2 = 0; i2 < 9; i2++) {
                int i3 = (iArr2[i] >> (i2 * 2)) & 3;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i, i2);
                if (ledItemRhyView != null) {
                    if (i3 == 1) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray1);
                    } else if (i3 == 2) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray2);
                    } else if (i3 == 3) {
                        ledItemRhyView.setChecked(true);
                        ledItemRhyView.setPaint(this.selectedColorGray3);
                    } else {
                        ledItemRhyView.setChecked(false);
                        ledItemRhyView.setPaint(this.unSelectedColor);
                    }
                    ledItemRhyView.postInvalidate();
                }
            }
        }
        this.index++;
    }

    public void setTimer(boolean z) {
        if (z) {
            MyTimeTask myTimeTask = new MyTimeTask(100L, new TimerTask() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmLedView.1
                @Override // java.util.TimerTask, java.lang.Runnable
                public void run() {
                    if (RhythmLedView.this.animEnable) {
                        RhythmLedView.this.handler.sendEmptyMessage(1001);
                    }
                }
            });
            this.task = myTimeTask;
            myTimeTask.start();
            return;
        }
        this.task.stop();
    }

    public void setShowAnim(boolean z) {
        this.animEnable = z;
    }

    public boolean getShowAnim() {
        return this.animEnable;
    }
}
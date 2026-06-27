package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.os.Handler;
import android.os.Message;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import cn.com.heaton.shiningmask.base.app.MyTimeTask;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import freemarker.core.FMParserConstants;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
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
    private int rhy1Color_25;
    private int rhy1Color_26;
    private int rhy1Color_27;
    private int rhy1Color_28;
    private int rhy1Color_29;
    private int rhy1Color_3;
    private int rhy1Color_30;
    private int rhy1Color_31;
    private int rhy1Color_32;
    private int rhy1Color_33;
    private int rhy1Color_34;
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
    private int rhy2Color_13;
    private int rhy2Color_14;
    private int rhy2Color_15;
    private int rhy2Color_16;
    private int rhy2Color_17;
    private int rhy2Color_18;
    private int rhy2Color_19;
    private int rhy2Color_2;
    private int rhy2Color_20;
    private int rhy2Color_21;
    private int rhy2Color_22;
    private int rhy2Color_23;
    private int rhy2Color_24;
    private int rhy2Color_25;
    private int rhy2Color_26;
    private int rhy2Color_27;
    private int rhy2Color_28;
    private int rhy2Color_29;
    private int rhy2Color_3;
    private int rhy2Color_30;
    private int rhy2Color_31;
    private int rhy2Color_32;
    private int rhy2Color_4;
    private int rhy2Color_5;
    private int rhy2Color_6;
    private int rhy2Color_7;
    private int rhy2Color_8;
    private int rhy2Color_9;
    private int rhy3Color_1;
    private int rhy3Color_10;
    private int rhy3Color_11;
    private int rhy3Color_12;
    private int rhy3Color_13;
    private int rhy3Color_14;
    private int rhy3Color_15;
    private int rhy3Color_16;
    private int rhy3Color_17;
    private int rhy3Color_18;
    private int rhy3Color_19;
    private int rhy3Color_2;
    private int rhy3Color_20;
    private int rhy3Color_21;
    private int rhy3Color_22;
    private int rhy3Color_23;
    private int rhy3Color_24;
    private int rhy3Color_25;
    private int rhy3Color_26;
    private int rhy3Color_3;
    private int rhy3Color_4;
    private int rhy3Color_5;
    private int rhy3Color_6;
    private int rhy3Color_7;
    private int rhy3Color_8;
    private int rhy3Color_9;
    private int rhy4Color_1;
    private int rhy4Color_10;
    private int rhy4Color_11;
    private int rhy4Color_12;
    private int rhy4Color_13;
    private int rhy4Color_14;
    private int rhy4Color_15;
    private int rhy4Color_16;
    private int rhy4Color_17;
    private int rhy4Color_18;
    private int rhy4Color_19;
    private int rhy4Color_2;
    private int rhy4Color_20;
    private int rhy4Color_21;
    private int rhy4Color_22;
    private int rhy4Color_23;
    private int rhy4Color_24;
    private int rhy4Color_25;
    private int rhy4Color_26;
    private int rhy4Color_3;
    private int rhy4Color_4;
    private int rhy4Color_5;
    private int rhy4Color_6;
    private int rhy4Color_7;
    private int rhy4Color_8;
    private int rhy4Color_9;
    private int rhy5Color_1;
    private int rhy5Color_10;
    private int rhy5Color_11;
    private int rhy5Color_12;
    private int rhy5Color_13;
    private int rhy5Color_14;
    private int rhy5Color_15;
    private int rhy5Color_16;
    private int rhy5Color_17;
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
        this.rhy1Color_1 = Color.rgb(255, 0, 127);
        this.rhy1Color_2 = Color.rgb(255, 0, 127);
        this.rhy1Color_3 = Color.rgb(255, 0, 255);
        this.rhy1Color_4 = Color.rgb(255, 0, 255);
        this.rhy1Color_5 = Color.rgb(127, 0, 255);
        this.rhy1Color_6 = Color.rgb(127, 0, 255);
        this.rhy1Color_7 = Color.rgb(0, 0, 255);
        this.rhy1Color_8 = Color.rgb(0, 0, 255);
        this.rhy1Color_9 = Color.rgb(0, 255, 255);
        this.rhy1Color_10 = Color.rgb(0, 255, 255);
        this.rhy1Color_11 = Color.rgb(0, 255, 0);
        this.rhy1Color_12 = Color.rgb(0, 255, 0);
        this.rhy1Color_13 = Color.rgb(255, 255, 0);
        this.rhy1Color_14 = Color.rgb(255, 255, 0);
        this.rhy1Color_15 = Color.rgb(255, 127, 0);
        this.rhy1Color_16 = Color.rgb(255, 127, 0);
        this.rhy1Color_17 = Color.rgb(255, 8, 0);
        this.rhy1Color_18 = Color.rgb(255, 8, 0);
        this.rhy1Color_19 = Color.rgb(255, 127, 0);
        this.rhy1Color_20 = Color.rgb(255, 127, 0);
        this.rhy1Color_21 = Color.rgb(255, 255, 0);
        this.rhy1Color_22 = Color.rgb(255, 255, 0);
        this.rhy1Color_23 = Color.rgb(0, 255, 0);
        this.rhy1Color_24 = Color.rgb(0, 255, 0);
        this.rhy1Color_25 = Color.rgb(0, 255, 255);
        this.rhy1Color_26 = Color.rgb(0, 255, 255);
        this.rhy1Color_27 = Color.rgb(0, 0, 255);
        this.rhy1Color_28 = Color.rgb(0, 0, 255);
        this.rhy1Color_29 = Color.rgb(127, 0, 255);
        this.rhy1Color_30 = Color.rgb(127, 0, 255);
        this.rhy1Color_31 = Color.rgb(255, 0, 255);
        this.rhy1Color_32 = Color.rgb(255, 0, 255);
        this.rhy1Color_33 = Color.rgb(255, 0, 127);
        this.rhy1Color_34 = Color.rgb(255, 0, 127);
        this.rhy2Color_1 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_2 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_3 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_4 = Color.rgb(255, 0, 240);
        this.rhy2Color_5 = Color.rgb(255, 0, 240);
        this.rhy2Color_6 = Color.rgb(216, 0, 255);
        this.rhy2Color_7 = Color.rgb(216, 0, 255);
        this.rhy2Color_8 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_9 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_10 = Color.rgb(132, 0, 255);
        this.rhy2Color_11 = Color.rgb(132, 0, 255);
        this.rhy2Color_12 = Color.rgb(102, 0, 255);
        this.rhy2Color_13 = Color.rgb(102, 0, 255);
        this.rhy2Color_14 = Color.rgb(0, 36, 255);
        this.rhy2Color_15 = Color.rgb(0, 36, 255);
        this.rhy2Color_16 = Color.rgb(0, 132, 255);
        this.rhy2Color_17 = Color.rgb(0, 132, 255);
        this.rhy2Color_18 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_19 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_20 = Color.rgb(0, 255, 234);
        this.rhy2Color_21 = Color.rgb(0, 255, 234);
        this.rhy2Color_22 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_23 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_24 = Color.rgb(126, 255, 0);
        this.rhy2Color_25 = Color.rgb(126, 255, 0);
        this.rhy2Color_26 = Color.rgb(246, 255, 0);
        this.rhy2Color_27 = Color.rgb(246, 255, 0);
        this.rhy2Color_28 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_29 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_30 = Color.rgb(255, 108, 0);
        this.rhy2Color_31 = Color.rgb(255, 108, 0);
        this.rhy2Color_32 = Color.rgb(255, 0, 0);
        this.rhy3Color_1 = Color.rgb(255, 246, 0);
        this.rhy3Color_2 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_3 = Color.rgb(255, 78, 0);
        this.rhy3Color_4 = Color.rgb(255, 24, 0);
        this.rhy3Color_5 = Color.rgb(255, 0, 102);
        this.rhy3Color_6 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_7 = Color.rgb(234, 0, 255);
        this.rhy3Color_8 = Color.rgb(139, 0, 237);
        this.rhy3Color_9 = Color.rgb(78, 0, 255);
        this.rhy3Color_10 = Color.rgb(0, 126, 255);
        this.rhy3Color_11 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_12 = Color.rgb(0, 240, 255);
        this.rhy3Color_13 = Color.rgb(0, 255, 255);
        this.rhy3Color_14 = Color.rgb(0, 255, 255);
        this.rhy3Color_15 = Color.rgb(0, 240, 255);
        this.rhy3Color_16 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_17 = Color.rgb(0, 126, 255);
        this.rhy3Color_18 = Color.rgb(78, 0, 255);
        this.rhy3Color_19 = Color.rgb(139, 0, 237);
        this.rhy3Color_20 = Color.rgb(234, 0, 255);
        this.rhy3Color_21 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_22 = Color.rgb(255, 0, 102);
        this.rhy3Color_23 = Color.rgb(255, 24, 0);
        this.rhy3Color_24 = Color.rgb(255, 78, 0);
        this.rhy3Color_25 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_26 = Color.rgb(255, 246, 0);
        this.rhy4Color_1 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy4Color_2 = Color.rgb(0, 240, 255);
        this.rhy4Color_3 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_4 = Color.rgb(0, 126, 255);
        this.rhy4Color_5 = Color.rgb(78, 0, 255);
        this.rhy4Color_6 = Color.rgb(139, 0, 237);
        this.rhy4Color_7 = Color.rgb(234, 0, 255);
        this.rhy4Color_8 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_9 = Color.rgb(255, 0, 102);
        this.rhy4Color_10 = Color.rgb(255, 24, 0);
        this.rhy4Color_11 = Color.rgb(255, 78, 0);
        this.rhy4Color_12 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_13 = Color.rgb(255, 246, 0);
        this.rhy4Color_14 = Color.rgb(255, 246, 0);
        this.rhy4Color_15 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_16 = Color.rgb(255, 78, 0);
        this.rhy4Color_17 = Color.rgb(255, 24, 0);
        this.rhy4Color_18 = Color.rgb(255, 0, 102);
        this.rhy4Color_19 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_20 = Color.rgb(234, 0, 255);
        this.rhy4Color_21 = Color.rgb(139, 0, 237);
        this.rhy4Color_22 = Color.rgb(78, 0, 255);
        this.rhy4Color_23 = Color.rgb(0, 126, 255);
        this.rhy4Color_24 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_25 = Color.rgb(0, 240, 255);
        this.rhy4Color_26 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy5Color_1 = Color.rgb(255, 0, 255);
        this.rhy5Color_2 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_3 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_4 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_5 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_6 = Color.rgb(69, 39, 255);
        this.rhy5Color_7 = Color.rgb(69, 39, 255);
        this.rhy5Color_8 = Color.rgb(1, 109, 255);
        this.rhy5Color_9 = Color.rgb(1, 109, 255);
        this.rhy5Color_10 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_11 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_12 = Color.rgb(1, 255, 234);
        this.rhy5Color_13 = Color.rgb(1, 255, 234);
        this.rhy5Color_14 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_15 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_16 = Color.rgb(48, 229, 124);
        this.rhy5Color_17 = Color.rgb(48, 229, 124);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.RhythmLedView.2
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
        this.rhy1Color_1 = Color.rgb(255, 0, 127);
        this.rhy1Color_2 = Color.rgb(255, 0, 127);
        this.rhy1Color_3 = Color.rgb(255, 0, 255);
        this.rhy1Color_4 = Color.rgb(255, 0, 255);
        this.rhy1Color_5 = Color.rgb(127, 0, 255);
        this.rhy1Color_6 = Color.rgb(127, 0, 255);
        this.rhy1Color_7 = Color.rgb(0, 0, 255);
        this.rhy1Color_8 = Color.rgb(0, 0, 255);
        this.rhy1Color_9 = Color.rgb(0, 255, 255);
        this.rhy1Color_10 = Color.rgb(0, 255, 255);
        this.rhy1Color_11 = Color.rgb(0, 255, 0);
        this.rhy1Color_12 = Color.rgb(0, 255, 0);
        this.rhy1Color_13 = Color.rgb(255, 255, 0);
        this.rhy1Color_14 = Color.rgb(255, 255, 0);
        this.rhy1Color_15 = Color.rgb(255, 127, 0);
        this.rhy1Color_16 = Color.rgb(255, 127, 0);
        this.rhy1Color_17 = Color.rgb(255, 8, 0);
        this.rhy1Color_18 = Color.rgb(255, 8, 0);
        this.rhy1Color_19 = Color.rgb(255, 127, 0);
        this.rhy1Color_20 = Color.rgb(255, 127, 0);
        this.rhy1Color_21 = Color.rgb(255, 255, 0);
        this.rhy1Color_22 = Color.rgb(255, 255, 0);
        this.rhy1Color_23 = Color.rgb(0, 255, 0);
        this.rhy1Color_24 = Color.rgb(0, 255, 0);
        this.rhy1Color_25 = Color.rgb(0, 255, 255);
        this.rhy1Color_26 = Color.rgb(0, 255, 255);
        this.rhy1Color_27 = Color.rgb(0, 0, 255);
        this.rhy1Color_28 = Color.rgb(0, 0, 255);
        this.rhy1Color_29 = Color.rgb(127, 0, 255);
        this.rhy1Color_30 = Color.rgb(127, 0, 255);
        this.rhy1Color_31 = Color.rgb(255, 0, 255);
        this.rhy1Color_32 = Color.rgb(255, 0, 255);
        this.rhy1Color_33 = Color.rgb(255, 0, 127);
        this.rhy1Color_34 = Color.rgb(255, 0, 127);
        this.rhy2Color_1 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_2 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_3 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_4 = Color.rgb(255, 0, 240);
        this.rhy2Color_5 = Color.rgb(255, 0, 240);
        this.rhy2Color_6 = Color.rgb(216, 0, 255);
        this.rhy2Color_7 = Color.rgb(216, 0, 255);
        this.rhy2Color_8 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_9 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_10 = Color.rgb(132, 0, 255);
        this.rhy2Color_11 = Color.rgb(132, 0, 255);
        this.rhy2Color_12 = Color.rgb(102, 0, 255);
        this.rhy2Color_13 = Color.rgb(102, 0, 255);
        this.rhy2Color_14 = Color.rgb(0, 36, 255);
        this.rhy2Color_15 = Color.rgb(0, 36, 255);
        this.rhy2Color_16 = Color.rgb(0, 132, 255);
        this.rhy2Color_17 = Color.rgb(0, 132, 255);
        this.rhy2Color_18 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_19 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_20 = Color.rgb(0, 255, 234);
        this.rhy2Color_21 = Color.rgb(0, 255, 234);
        this.rhy2Color_22 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_23 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_24 = Color.rgb(126, 255, 0);
        this.rhy2Color_25 = Color.rgb(126, 255, 0);
        this.rhy2Color_26 = Color.rgb(246, 255, 0);
        this.rhy2Color_27 = Color.rgb(246, 255, 0);
        this.rhy2Color_28 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_29 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_30 = Color.rgb(255, 108, 0);
        this.rhy2Color_31 = Color.rgb(255, 108, 0);
        this.rhy2Color_32 = Color.rgb(255, 0, 0);
        this.rhy3Color_1 = Color.rgb(255, 246, 0);
        this.rhy3Color_2 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_3 = Color.rgb(255, 78, 0);
        this.rhy3Color_4 = Color.rgb(255, 24, 0);
        this.rhy3Color_5 = Color.rgb(255, 0, 102);
        this.rhy3Color_6 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_7 = Color.rgb(234, 0, 255);
        this.rhy3Color_8 = Color.rgb(139, 0, 237);
        this.rhy3Color_9 = Color.rgb(78, 0, 255);
        this.rhy3Color_10 = Color.rgb(0, 126, 255);
        this.rhy3Color_11 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_12 = Color.rgb(0, 240, 255);
        this.rhy3Color_13 = Color.rgb(0, 255, 255);
        this.rhy3Color_14 = Color.rgb(0, 255, 255);
        this.rhy3Color_15 = Color.rgb(0, 240, 255);
        this.rhy3Color_16 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_17 = Color.rgb(0, 126, 255);
        this.rhy3Color_18 = Color.rgb(78, 0, 255);
        this.rhy3Color_19 = Color.rgb(139, 0, 237);
        this.rhy3Color_20 = Color.rgb(234, 0, 255);
        this.rhy3Color_21 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_22 = Color.rgb(255, 0, 102);
        this.rhy3Color_23 = Color.rgb(255, 24, 0);
        this.rhy3Color_24 = Color.rgb(255, 78, 0);
        this.rhy3Color_25 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_26 = Color.rgb(255, 246, 0);
        this.rhy4Color_1 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy4Color_2 = Color.rgb(0, 240, 255);
        this.rhy4Color_3 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_4 = Color.rgb(0, 126, 255);
        this.rhy4Color_5 = Color.rgb(78, 0, 255);
        this.rhy4Color_6 = Color.rgb(139, 0, 237);
        this.rhy4Color_7 = Color.rgb(234, 0, 255);
        this.rhy4Color_8 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_9 = Color.rgb(255, 0, 102);
        this.rhy4Color_10 = Color.rgb(255, 24, 0);
        this.rhy4Color_11 = Color.rgb(255, 78, 0);
        this.rhy4Color_12 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_13 = Color.rgb(255, 246, 0);
        this.rhy4Color_14 = Color.rgb(255, 246, 0);
        this.rhy4Color_15 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_16 = Color.rgb(255, 78, 0);
        this.rhy4Color_17 = Color.rgb(255, 24, 0);
        this.rhy4Color_18 = Color.rgb(255, 0, 102);
        this.rhy4Color_19 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_20 = Color.rgb(234, 0, 255);
        this.rhy4Color_21 = Color.rgb(139, 0, 237);
        this.rhy4Color_22 = Color.rgb(78, 0, 255);
        this.rhy4Color_23 = Color.rgb(0, 126, 255);
        this.rhy4Color_24 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_25 = Color.rgb(0, 240, 255);
        this.rhy4Color_26 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy5Color_1 = Color.rgb(255, 0, 255);
        this.rhy5Color_2 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_3 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_4 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_5 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_6 = Color.rgb(69, 39, 255);
        this.rhy5Color_7 = Color.rgb(69, 39, 255);
        this.rhy5Color_8 = Color.rgb(1, 109, 255);
        this.rhy5Color_9 = Color.rgb(1, 109, 255);
        this.rhy5Color_10 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_11 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_12 = Color.rgb(1, 255, 234);
        this.rhy5Color_13 = Color.rgb(1, 255, 234);
        this.rhy5Color_14 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_15 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_16 = Color.rgb(48, 229, 124);
        this.rhy5Color_17 = Color.rgb(48, 229, 124);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.RhythmLedView.2
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
        this.rhy1Color_1 = Color.rgb(255, 0, 127);
        this.rhy1Color_2 = Color.rgb(255, 0, 127);
        this.rhy1Color_3 = Color.rgb(255, 0, 255);
        this.rhy1Color_4 = Color.rgb(255, 0, 255);
        this.rhy1Color_5 = Color.rgb(127, 0, 255);
        this.rhy1Color_6 = Color.rgb(127, 0, 255);
        this.rhy1Color_7 = Color.rgb(0, 0, 255);
        this.rhy1Color_8 = Color.rgb(0, 0, 255);
        this.rhy1Color_9 = Color.rgb(0, 255, 255);
        this.rhy1Color_10 = Color.rgb(0, 255, 255);
        this.rhy1Color_11 = Color.rgb(0, 255, 0);
        this.rhy1Color_12 = Color.rgb(0, 255, 0);
        this.rhy1Color_13 = Color.rgb(255, 255, 0);
        this.rhy1Color_14 = Color.rgb(255, 255, 0);
        this.rhy1Color_15 = Color.rgb(255, 127, 0);
        this.rhy1Color_16 = Color.rgb(255, 127, 0);
        this.rhy1Color_17 = Color.rgb(255, 8, 0);
        this.rhy1Color_18 = Color.rgb(255, 8, 0);
        this.rhy1Color_19 = Color.rgb(255, 127, 0);
        this.rhy1Color_20 = Color.rgb(255, 127, 0);
        this.rhy1Color_21 = Color.rgb(255, 255, 0);
        this.rhy1Color_22 = Color.rgb(255, 255, 0);
        this.rhy1Color_23 = Color.rgb(0, 255, 0);
        this.rhy1Color_24 = Color.rgb(0, 255, 0);
        this.rhy1Color_25 = Color.rgb(0, 255, 255);
        this.rhy1Color_26 = Color.rgb(0, 255, 255);
        this.rhy1Color_27 = Color.rgb(0, 0, 255);
        this.rhy1Color_28 = Color.rgb(0, 0, 255);
        this.rhy1Color_29 = Color.rgb(127, 0, 255);
        this.rhy1Color_30 = Color.rgb(127, 0, 255);
        this.rhy1Color_31 = Color.rgb(255, 0, 255);
        this.rhy1Color_32 = Color.rgb(255, 0, 255);
        this.rhy1Color_33 = Color.rgb(255, 0, 127);
        this.rhy1Color_34 = Color.rgb(255, 0, 127);
        this.rhy2Color_1 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_2 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_3 = Color.rgb(255, 0, Opcodes.FRETURN);
        this.rhy2Color_4 = Color.rgb(255, 0, 240);
        this.rhy2Color_5 = Color.rgb(255, 0, 240);
        this.rhy2Color_6 = Color.rgb(216, 0, 255);
        this.rhy2Color_7 = Color.rgb(216, 0, 255);
        this.rhy2Color_8 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_9 = Color.rgb(Opcodes.FRETURN, 0, 255);
        this.rhy2Color_10 = Color.rgb(132, 0, 255);
        this.rhy2Color_11 = Color.rgb(132, 0, 255);
        this.rhy2Color_12 = Color.rgb(102, 0, 255);
        this.rhy2Color_13 = Color.rgb(102, 0, 255);
        this.rhy2Color_14 = Color.rgb(0, 36, 255);
        this.rhy2Color_15 = Color.rgb(0, 36, 255);
        this.rhy2Color_16 = Color.rgb(0, 132, 255);
        this.rhy2Color_17 = Color.rgb(0, 132, 255);
        this.rhy2Color_18 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_19 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy2Color_20 = Color.rgb(0, 255, 234);
        this.rhy2Color_21 = Color.rgb(0, 255, 234);
        this.rhy2Color_22 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_23 = Color.rgb(0, 255, FMParserConstants.MINUS_MINUS);
        this.rhy2Color_24 = Color.rgb(126, 255, 0);
        this.rhy2Color_25 = Color.rgb(126, 255, 0);
        this.rhy2Color_26 = Color.rgb(246, 255, 0);
        this.rhy2Color_27 = Color.rgb(246, 255, 0);
        this.rhy2Color_28 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_29 = Color.rgb(255, HttpStatus.SC_NO_CONTENT, 0);
        this.rhy2Color_30 = Color.rgb(255, 108, 0);
        this.rhy2Color_31 = Color.rgb(255, 108, 0);
        this.rhy2Color_32 = Color.rgb(255, 0, 0);
        this.rhy3Color_1 = Color.rgb(255, 246, 0);
        this.rhy3Color_2 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_3 = Color.rgb(255, 78, 0);
        this.rhy3Color_4 = Color.rgb(255, 24, 0);
        this.rhy3Color_5 = Color.rgb(255, 0, 102);
        this.rhy3Color_6 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_7 = Color.rgb(234, 0, 255);
        this.rhy3Color_8 = Color.rgb(139, 0, 237);
        this.rhy3Color_9 = Color.rgb(78, 0, 255);
        this.rhy3Color_10 = Color.rgb(0, 126, 255);
        this.rhy3Color_11 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_12 = Color.rgb(0, 240, 255);
        this.rhy3Color_13 = Color.rgb(0, 255, 255);
        this.rhy3Color_14 = Color.rgb(0, 255, 255);
        this.rhy3Color_15 = Color.rgb(0, 240, 255);
        this.rhy3Color_16 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy3Color_17 = Color.rgb(0, 126, 255);
        this.rhy3Color_18 = Color.rgb(78, 0, 255);
        this.rhy3Color_19 = Color.rgb(139, 0, 237);
        this.rhy3Color_20 = Color.rgb(234, 0, 255);
        this.rhy3Color_21 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy3Color_22 = Color.rgb(255, 0, 102);
        this.rhy3Color_23 = Color.rgb(255, 24, 0);
        this.rhy3Color_24 = Color.rgb(255, 78, 0);
        this.rhy3Color_25 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy3Color_26 = Color.rgb(255, 246, 0);
        this.rhy4Color_1 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy4Color_2 = Color.rgb(0, 240, 255);
        this.rhy4Color_3 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_4 = Color.rgb(0, 126, 255);
        this.rhy4Color_5 = Color.rgb(78, 0, 255);
        this.rhy4Color_6 = Color.rgb(139, 0, 237);
        this.rhy4Color_7 = Color.rgb(234, 0, 255);
        this.rhy4Color_8 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_9 = Color.rgb(255, 0, 102);
        this.rhy4Color_10 = Color.rgb(255, 24, 0);
        this.rhy4Color_11 = Color.rgb(255, 78, 0);
        this.rhy4Color_12 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_13 = Color.rgb(255, 246, 0);
        this.rhy4Color_14 = Color.rgb(255, 246, 0);
        this.rhy4Color_15 = Color.rgb(255, Opcodes.FRETURN, 0);
        this.rhy4Color_16 = Color.rgb(255, 78, 0);
        this.rhy4Color_17 = Color.rgb(255, 24, 0);
        this.rhy4Color_18 = Color.rgb(255, 0, 102);
        this.rhy4Color_19 = Color.rgb(255, 0, Opcodes.IF_ICMPGE);
        this.rhy4Color_20 = Color.rgb(234, 0, 255);
        this.rhy4Color_21 = Color.rgb(139, 0, 237);
        this.rhy4Color_22 = Color.rgb(78, 0, 255);
        this.rhy4Color_23 = Color.rgb(0, 126, 255);
        this.rhy4Color_24 = Color.rgb(0, Opcodes.CHECKCAST, 255);
        this.rhy4Color_25 = Color.rgb(0, 240, 255);
        this.rhy4Color_26 = Color.rgb(FMParserConstants.LESS_THAN, 255, 253);
        this.rhy5Color_1 = Color.rgb(255, 0, 255);
        this.rhy5Color_2 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_3 = Color.rgb(HttpStatus.SC_NO_CONTENT, 0, 255);
        this.rhy5Color_4 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_5 = Color.rgb(FMParserConstants.NON_ESCAPED_ID_START_CHAR, 0, 255);
        this.rhy5Color_6 = Color.rgb(69, 39, 255);
        this.rhy5Color_7 = Color.rgb(69, 39, 255);
        this.rhy5Color_8 = Color.rgb(1, 109, 255);
        this.rhy5Color_9 = Color.rgb(1, 109, 255);
        this.rhy5Color_10 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_11 = Color.rgb(1, Opcodes.CHECKCAST, 255);
        this.rhy5Color_12 = Color.rgb(1, 255, 234);
        this.rhy5Color_13 = Color.rgb(1, 255, 234);
        this.rhy5Color_14 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_15 = Color.rgb(1, 255, Opcodes.JSR);
        this.rhy5Color_16 = Color.rgb(48, 229, 124);
        this.rhy5Color_17 = Color.rgb(48, 229, 124);
        this.index = 0;
        this.handler = new Handler() { // from class: cn.com.heaton.shiningmask.ui.widget.RhythmLedView.2
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

    private LedItemRhyView getLedItemRhyView(int i, int i2) {
        return (LedItemRhyView) getChildAt((i * 32) + (32 - i2));
    }

    public void updateRhythmUI(List<Integer> list) {
        if (list != null) {
            clearSelected();
        }
        for (int i = 1; i <= 17; i++) {
            int iIntValue = list.get(i).intValue();
            if (iIntValue == 0) {
                iIntValue = 2;
            }
            if (iIntValue > 30) {
                return;
            }
            for (int i2 = 1; i2 <= iIntValue; i2++) {
                int i3 = i * 3;
                int i4 = i3 - 2;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i3 - 1, i2);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4, i2);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                switch (i) {
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
                    case 13:
                        ledItemRhyView.setPaint(this.rhy1Color_25);
                        ledItemRhyView2.setPaint(this.rhy1Color_26);
                        break;
                    case 14:
                        ledItemRhyView.setPaint(this.rhy1Color_27);
                        ledItemRhyView2.setPaint(this.rhy1Color_28);
                        break;
                    case 15:
                        ledItemRhyView.setPaint(this.rhy1Color_29);
                        ledItemRhyView2.setPaint(this.rhy1Color_30);
                        break;
                    case 16:
                        ledItemRhyView.setPaint(this.rhy1Color_31);
                        ledItemRhyView2.setPaint(this.rhy1Color_32);
                        break;
                    case 17:
                        ledItemRhyView.setPaint(this.rhy1Color_33);
                        ledItemRhyView2.setPaint(this.rhy1Color_34);
                        break;
                }
            }
        }
    }

    public void setRhyData2(List<Integer> list) {
        if (list == null) {
            return;
        }
        clearSelected();
        for (int i = 1; i <= 25; i++) {
            int iIntValue = list.get(i - 1).intValue();
            if (iIntValue == 0) {
                iIntValue = 2;
            }
            if (iIntValue > 30) {
                return;
            }
            for (int i2 = 1; i2 <= iIntValue; i2++) {
                int i3 = i * 2;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i3 - 1, i2);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i3, i2);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                ledItemRhyView.setPaint(this.rhy2Color_32);
                ledItemRhyView2.setPaint(this.rhy2Color_32);
                switch (i2) {
                    case 1:
                        ledItemRhyView.setPaint(this.rhy2Color_32);
                        ledItemRhyView2.setPaint(this.rhy2Color_32);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy2Color_32);
                        ledItemRhyView2.setPaint(this.rhy2Color_32);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy2Color_32);
                        ledItemRhyView2.setPaint(this.rhy2Color_32);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy2Color_31);
                        ledItemRhyView2.setPaint(this.rhy2Color_31);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy2Color_30);
                        ledItemRhyView2.setPaint(this.rhy2Color_30);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy2Color_29);
                        ledItemRhyView2.setPaint(this.rhy2Color_29);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy2Color_28);
                        ledItemRhyView2.setPaint(this.rhy2Color_28);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy2Color_27);
                        ledItemRhyView2.setPaint(this.rhy2Color_27);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy2Color_26);
                        ledItemRhyView2.setPaint(this.rhy2Color_26);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy2Color_25);
                        ledItemRhyView2.setPaint(this.rhy2Color_25);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy2Color_24);
                        ledItemRhyView2.setPaint(this.rhy2Color_24);
                        break;
                    case 12:
                        ledItemRhyView.setPaint(this.rhy2Color_23);
                        ledItemRhyView2.setPaint(this.rhy2Color_23);
                        break;
                    case 13:
                        ledItemRhyView.setPaint(this.rhy2Color_22);
                        ledItemRhyView2.setPaint(this.rhy2Color_22);
                        break;
                    case 14:
                        ledItemRhyView.setPaint(this.rhy2Color_21);
                        ledItemRhyView2.setPaint(this.rhy2Color_21);
                        break;
                    case 15:
                        ledItemRhyView.setPaint(this.rhy2Color_20);
                        ledItemRhyView2.setPaint(this.rhy2Color_20);
                        break;
                    case 16:
                        ledItemRhyView.setPaint(this.rhy2Color_19);
                        ledItemRhyView2.setPaint(this.rhy2Color_19);
                        break;
                    case 17:
                        ledItemRhyView.setPaint(this.rhy2Color_18);
                        ledItemRhyView2.setPaint(this.rhy2Color_18);
                        break;
                    case 18:
                        ledItemRhyView.setPaint(this.rhy2Color_17);
                        ledItemRhyView2.setPaint(this.rhy2Color_17);
                        break;
                    case 19:
                        ledItemRhyView.setPaint(this.rhy2Color_16);
                        ledItemRhyView2.setPaint(this.rhy2Color_16);
                        break;
                    case 20:
                        ledItemRhyView.setPaint(this.rhy2Color_15);
                        ledItemRhyView2.setPaint(this.rhy2Color_15);
                        break;
                    case 21:
                        ledItemRhyView.setPaint(this.rhy2Color_14);
                        ledItemRhyView2.setPaint(this.rhy2Color_14);
                        break;
                    case 22:
                        ledItemRhyView.setPaint(this.rhy2Color_13);
                        ledItemRhyView2.setPaint(this.rhy2Color_13);
                        break;
                    case 23:
                        ledItemRhyView.setPaint(this.rhy2Color_13);
                        ledItemRhyView2.setPaint(this.rhy2Color_13);
                        break;
                    case 24:
                        ledItemRhyView.setPaint(this.rhy2Color_12);
                        ledItemRhyView2.setPaint(this.rhy2Color_12);
                        break;
                    case 25:
                        ledItemRhyView.setPaint(this.rhy2Color_11);
                        ledItemRhyView2.setPaint(this.rhy2Color_11);
                        break;
                    case 26:
                        ledItemRhyView.setPaint(this.rhy2Color_10);
                        ledItemRhyView2.setPaint(this.rhy2Color_10);
                        break;
                    case 27:
                        ledItemRhyView.setPaint(this.rhy2Color_9);
                        ledItemRhyView2.setPaint(this.rhy2Color_9);
                        break;
                    case 28:
                        ledItemRhyView.setPaint(this.rhy2Color_8);
                        ledItemRhyView2.setPaint(this.rhy2Color_8);
                        break;
                    case 29:
                        ledItemRhyView.setPaint(this.rhy2Color_7);
                        ledItemRhyView2.setPaint(this.rhy2Color_7);
                        break;
                    case 30:
                        ledItemRhyView.setPaint(this.rhy2Color_6);
                        ledItemRhyView2.setPaint(this.rhy2Color_6);
                        break;
                    case 31:
                        ledItemRhyView.setPaint(this.rhy2Color_5);
                        ledItemRhyView2.setPaint(this.rhy2Color_5);
                        break;
                    case 32:
                        ledItemRhyView.setPaint(this.rhy2Color_1);
                        ledItemRhyView2.setPaint(this.rhy2Color_1);
                        break;
                    default:
                        ledItemRhyView.setPaint(this.rhy2Color_32);
                        ledItemRhyView2.setPaint(this.rhy2Color_32);
                        break;
                }
            }
        }
    }

    public void setRhyData3(List<Integer> list) {
        if (list == null) {
            return;
        }
        clearSelected();
        for (int i = 1; i <= 26; i++) {
            int iIntValue = list.get(i - 1).intValue();
            if (iIntValue == 0) {
                iIntValue = 2;
            }
            if (iIntValue > 30) {
                return;
            }
            for (int i2 = 1; i2 <= iIntValue; i2++) {
                int i3 = i * 2;
                int i4 = i3 - 1;
                if (i4 >= 26) {
                    i4 = i3 - 2;
                }
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4, i2);
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.rhy1Color_1);
                switch (i) {
                    case 1:
                        ledItemRhyView.setPaint(this.rhy3Color_1);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy3Color_2);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy3Color_3);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy3Color_4);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy3Color_5);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy3Color_6);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy3Color_7);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy3Color_8);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy3Color_9);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy3Color_10);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy3Color_11);
                        break;
                    case 12:
                        ledItemRhyView.setPaint(this.rhy3Color_12);
                        break;
                    case 13:
                        ledItemRhyView.setPaint(this.rhy3Color_13);
                        break;
                    case 14:
                        ledItemRhyView.setPaint(this.rhy3Color_14);
                        break;
                    case 15:
                        ledItemRhyView.setPaint(this.rhy3Color_15);
                        break;
                    case 16:
                        ledItemRhyView.setPaint(this.rhy3Color_16);
                        break;
                    case 17:
                        ledItemRhyView.setPaint(this.rhy3Color_17);
                        break;
                    case 18:
                        ledItemRhyView.setPaint(this.rhy3Color_18);
                        break;
                    case 19:
                        ledItemRhyView.setPaint(this.rhy3Color_19);
                        break;
                    case 20:
                        ledItemRhyView.setPaint(this.rhy3Color_20);
                        break;
                    case 21:
                        ledItemRhyView.setPaint(this.rhy3Color_21);
                        break;
                    case 22:
                        ledItemRhyView.setPaint(this.rhy3Color_22);
                        break;
                    case 23:
                        ledItemRhyView.setPaint(this.rhy3Color_23);
                        break;
                    case 24:
                        ledItemRhyView.setPaint(this.rhy3Color_24);
                        break;
                    case 25:
                        ledItemRhyView.setPaint(this.rhy3Color_25);
                        break;
                    case 26:
                        ledItemRhyView.setPaint(this.rhy3Color_26);
                        break;
                }
            }
        }
    }

    public void setRhyData4(List<Integer> list) {
        if (list == null) {
            return;
        }
        clearSelected();
        for (int i = 1; i <= 26; i++) {
            int iIntValue = list.get(i - 1).intValue();
            if (iIntValue == 0) {
                iIntValue = 2;
            }
            if (iIntValue > 30) {
                return;
            }
            for (int i2 = 1; i2 <= iIntValue; i2++) {
                int i3 = i * 2;
                int i4 = i3 - 1;
                if (i4 >= 26) {
                    i4 = i3 - 2;
                }
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i4, i2);
                ledItemRhyView.setChecked(true);
                ledItemRhyView.setPaint(this.rhy1Color_1);
                switch (i) {
                    case 1:
                        ledItemRhyView.setPaint(this.rhy4Color_1);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy4Color_2);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy4Color_3);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy4Color_4);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy4Color_5);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy4Color_6);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy4Color_7);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy4Color_8);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy4Color_9);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy4Color_10);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy4Color_11);
                        break;
                    case 12:
                        ledItemRhyView.setPaint(this.rhy4Color_12);
                        break;
                    case 13:
                        ledItemRhyView.setPaint(this.rhy4Color_13);
                        break;
                    case 14:
                        ledItemRhyView.setPaint(this.rhy4Color_14);
                        break;
                    case 15:
                        ledItemRhyView.setPaint(this.rhy4Color_15);
                        break;
                    case 16:
                        ledItemRhyView.setPaint(this.rhy4Color_16);
                        break;
                    case 17:
                        ledItemRhyView.setPaint(this.rhy4Color_17);
                        break;
                    case 18:
                        ledItemRhyView.setPaint(this.rhy4Color_18);
                        break;
                    case 19:
                        ledItemRhyView.setPaint(this.rhy4Color_19);
                        break;
                    case 20:
                        ledItemRhyView.setPaint(this.rhy4Color_20);
                        break;
                    case 21:
                        ledItemRhyView.setPaint(this.rhy4Color_21);
                        break;
                    case 22:
                        ledItemRhyView.setPaint(this.rhy4Color_22);
                        break;
                    case 23:
                        ledItemRhyView.setPaint(this.rhy4Color_23);
                        break;
                    case 24:
                        ledItemRhyView.setPaint(this.rhy4Color_24);
                        break;
                    case 25:
                        ledItemRhyView.setPaint(this.rhy4Color_25);
                        break;
                    case 26:
                        ledItemRhyView.setPaint(this.rhy4Color_26);
                        break;
                }
            }
        }
    }

    public void setRhyData5(List<Integer> list) {
        if (list == null) {
            return;
        }
        clearSelected();
        for (int i = 1; i <= 17; i++) {
            int iIntValue = list.get(i).intValue();
            if (iIntValue == 0) {
                iIntValue = 2;
            }
            if (iIntValue > 30) {
                return;
            }
            for (int i2 = 1; i2 <= iIntValue; i2++) {
                int i3 = i * 3;
                int i4 = i3 - 2;
                LedItemRhyView ledItemRhyView = getLedItemRhyView(i3 - 1, i2);
                LedItemRhyView ledItemRhyView2 = getLedItemRhyView(i4, i2);
                ledItemRhyView.setChecked(true);
                ledItemRhyView2.setChecked(true);
                ledItemRhyView.setPaint(this.rhy1Color_1);
                ledItemRhyView2.setPaint(this.rhy1Color_2);
                switch (i) {
                    case 1:
                        ledItemRhyView.setPaint(this.rhy5Color_17);
                        ledItemRhyView2.setPaint(this.rhy5Color_16);
                        break;
                    case 2:
                        ledItemRhyView.setPaint(this.rhy5Color_15);
                        ledItemRhyView2.setPaint(this.rhy5Color_14);
                        break;
                    case 3:
                        ledItemRhyView.setPaint(this.rhy5Color_13);
                        ledItemRhyView2.setPaint(this.rhy5Color_12);
                        break;
                    case 4:
                        ledItemRhyView.setPaint(this.rhy5Color_11);
                        ledItemRhyView2.setPaint(this.rhy5Color_10);
                        break;
                    case 5:
                        ledItemRhyView.setPaint(this.rhy5Color_9);
                        ledItemRhyView2.setPaint(this.rhy5Color_8);
                        break;
                    case 6:
                        ledItemRhyView.setPaint(this.rhy5Color_7);
                        ledItemRhyView2.setPaint(this.rhy5Color_6);
                        break;
                    case 7:
                        ledItemRhyView.setPaint(this.rhy5Color_5);
                        ledItemRhyView2.setPaint(this.rhy5Color_4);
                        break;
                    case 8:
                        ledItemRhyView.setPaint(this.rhy5Color_3);
                        ledItemRhyView2.setPaint(this.rhy5Color_2);
                        break;
                    case 9:
                        ledItemRhyView.setPaint(this.rhy5Color_1);
                        ledItemRhyView2.setPaint(this.rhy5Color_1);
                        break;
                    case 10:
                        ledItemRhyView.setPaint(this.rhy5Color_2);
                        ledItemRhyView2.setPaint(this.rhy5Color_3);
                        break;
                    case 11:
                        ledItemRhyView.setPaint(this.rhy5Color_4);
                        ledItemRhyView2.setPaint(this.rhy5Color_5);
                        break;
                    case 12:
                        ledItemRhyView.setPaint(this.rhy5Color_6);
                        ledItemRhyView2.setPaint(this.rhy5Color_7);
                        break;
                    case 13:
                        ledItemRhyView.setPaint(this.rhy5Color_8);
                        ledItemRhyView2.setPaint(this.rhy5Color_9);
                        break;
                    case 14:
                        ledItemRhyView.setPaint(this.rhy5Color_10);
                        ledItemRhyView2.setPaint(this.rhy5Color_11);
                        break;
                    case 15:
                        ledItemRhyView.setPaint(this.rhy5Color_12);
                        ledItemRhyView2.setPaint(this.rhy5Color_13);
                        break;
                    case 16:
                        ledItemRhyView.setPaint(this.rhy5Color_14);
                        ledItemRhyView2.setPaint(this.rhy5Color_15);
                        break;
                    case 17:
                        ledItemRhyView.setPaint(this.rhy5Color_16);
                        ledItemRhyView2.setPaint(this.rhy5Color_17);
                        break;
                }
            }
        }
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
            MyTimeTask myTimeTask = new MyTimeTask(100L, new TimerTask() { // from class: cn.com.heaton.shiningmask.ui.widget.RhythmLedView.1
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

    @Override // android.view.ViewGroup, android.view.View
    protected void dispatchDraw(Canvas canvas) {
        super.dispatchDraw(canvas);
    }
}
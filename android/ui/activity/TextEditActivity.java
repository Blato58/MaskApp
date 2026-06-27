package cn.com.heaton.shiningmask.ui.activity;

import android.content.DialogInterface;
import android.content.res.TypedArray;
import android.graphics.Color;
import android.graphics.drawable.AnimationDrawable;
import android.os.Handler;
import android.text.TextUtils;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AlphaAnimation;
import android.view.inputmethod.InputMethodManager;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.appcompat.app.AlertDialog;
import androidx.recyclerview.widget.GridLayoutManager;
import androidx.recyclerview.widget.LinearLayoutManager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.app.SoundManager;
import cn.com.heaton.shiningmask.base.music.MusicPlayer;
import cn.com.heaton.shiningmask.dao.DaoSession;
import cn.com.heaton.shiningmask.dao.HistoryDataDao;
import cn.com.heaton.shiningmask.databinding.ActivityTextEdit2Binding;
import cn.com.heaton.shiningmask.model.bean.HistoryData;
import cn.com.heaton.shiningmask.model.bean.TextData;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.model.data.Text1664Bold;
import cn.com.heaton.shiningmask.model.data.TextAgreement;
import cn.com.heaton.shiningmask.model.data.TextIconData;
import cn.com.heaton.shiningmask.ui.adapter.HistoryListAdapter;
import cn.com.heaton.shiningmask.ui.adapter.LedViewAdapter;
import cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.DensityUtil;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import cn.com.heaton.shiningmask.ui.utils.SoftKeyboardStateHelper;
import cn.com.heaton.shiningmask.ui.widget.MultiLineRadioGroup;
import cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleManager;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class TextEditActivity extends BaseActivity<ActivityTextEdit2Binding> implements View.OnClickListener {
    private AnimationDrawable animMode;
    private BleManager bleManager;
    private int curAnimType;
    private int[] curDataColorArray;
    private byte[] curLedData;
    private float curTextAngle;
    private float curTextBgAngle;
    private int curTextBgColorB;
    private int curTextBgColorG;
    private int curTextBgColorR;
    private int curTextColorB;
    private int curTextColorG;
    private int curTextColorR;
    private DaoSession daoSession;
    private HistoryDataDao historyDataDao;
    private HistoryListAdapter historyListAdapter;
    private boolean isShowAddText;
    private boolean isShowHistorical;
    private LedViewAdapter ledViewAdapter;
    private MusicPlayer musicPlayer;
    private TextAgreement textAgreement;
    private boolean textColorBgSelectEnable;
    private boolean textColorSelectEnable;
    private TextImageIconAdapter textImageIconAdapter;
    private List<BleDevice> deviceList = new ArrayList();
    private List<TextData> textList = new ArrayList();
    private List<byte[]> iconDataList = new ArrayList();
    private int curAminMode = 0;
    private List<Integer> imageList = new ArrayList();
    private int gradientMode = 0;
    private int bgColorMode = 0;
    Handler mHandler = new Handler();
    private int curTextColor = -788529156;
    private int curAddTextColor = -788529156;
    private int curTextBgColor = Color.rgb(127, 127, 127);
    private LinkedList<HistoryData> historyDataList = new LinkedList<>();
    private int curSpeed = 50;

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityTextEdit2Binding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityTextEdit2Binding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivModePrevious.setOnClickListener(this);
        getBinding().ivModeNext.setOnClickListener(this);
        getBinding().ibtnSend.setOnClickListener(this);
        getBinding().llTextEdit.setOnClickListener(this);
        getBinding().top.ivBack.setOnClickListener(this);
        getBinding().llLedViewPreview.setOnClickListener(this);
        getBinding().ivOk.setOnClickListener(this);
        getBinding().rvLedviewList.setOnClickListener(this);
        getBinding().ivGo.setOnClickListener(this);
        getBinding().top.ivForward.setOnClickListener(this);
        getBinding().viewLed.setOnClickListener(this);
        getBinding().top.ivForward1.setOnClickListener(this);
        getBinding().viewTextPickcolor1.setOnClickListener(this);
        getBinding().viewTextPickcolor2.setOnClickListener(this);
        getBinding().top.tvTitle.setText(getResources().getText(R.string.main_text));
        getBinding().top.ivForward1.setVisibility(0);
        getBinding().top.ivForward1.setImageResource(R.mipmap.text_history_icon);
        this.textColorSelectEnable = DataManager.getInstance().isTextColorEnable();
        this.textColorBgSelectEnable = DataManager.getInstance().isTextColorBgEnable();
        this.curTextColor = DataManager.getInstance().getTextColor();
        this.curTextBgColor = DataManager.getInstance().getTextBgColor();
        this.curTextColorR = Color.red(this.curTextColor);
        this.curTextColorG = Color.green(this.curTextColor);
        this.curTextColorB = Color.blue(this.curTextColor);
        this.curTextBgColorR = Color.red(this.curTextBgColor);
        this.curTextBgColorG = Color.green(this.curTextBgColor);
        this.curTextBgColorB = Color.blue(this.curTextBgColor);
        this.curTextAngle = DataManager.getInstance().getTextColorAngle();
        this.curTextBgAngle = DataManager.getInstance().getTextColorBgAngle();
        this.gradientMode = DataManager.getInstance().getTextColorMode();
        this.bgColorMode = DataManager.getInstance().getTextColorBgMode();
        LogUtil.d("gradientMode:" + this.gradientMode + " bgColorMode:" + this.bgColorMode + " textColorSelectEnable:" + this.textColorSelectEnable + " textColorBgSelectEnable:" + this.textColorBgSelectEnable);
        GridLayoutManager gridLayoutManager = new GridLayoutManager(this.mContext, 2);
        gridLayoutManager.setOrientation(0);
        gridLayoutManager.offsetChildrenVertical(ScreenUtils.dp2px(this.mContext, 5.0f));
        getBinding().rvImageIconList.setLayoutManager(gridLayoutManager);
        LinearLayoutManager linearLayoutManager = new LinearLayoutManager(this);
        linearLayoutManager.setOrientation(0);
        getBinding().rvLedviewList.setLayoutManager(linearLayoutManager);
        int[] textIconData = getTextIconData(R.array.text_input_expression);
        for (int i = 0; i < textIconData.length / 2; i++) {
            int length = (textIconData.length / 2) + i;
            this.imageList.add(Integer.valueOf(textIconData[i]));
            this.imageList.add(Integer.valueOf(textIconData[length]));
        }
        this.textImageIconAdapter = new TextImageIconAdapter(this, R.layout.item_text_icon_image, this.imageList);
        getBinding().rvImageIconList.setAdapter(this.textImageIconAdapter);
        this.ledViewAdapter = new LedViewAdapter(this, R.layout.item_ledview, this.textList);
        getBinding().rvLedviewList.setAdapter(this.ledViewAdapter);
        this.ledViewAdapter.setOnItemClickListener(new RecyclerAdapter.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.1
            @Override // cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter.OnItemClickListener
            public void onItemClick(ViewGroup viewGroup, View view, Object obj, int i2) {
                TextEditActivity.this.showKey();
            }
        });
        new SoftKeyboardStateHelper(getBinding().llTextAdd).addSoftKeyboardStateListener(new SoftKeyboardStateHelper.SoftKeyboardStateListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.2
            @Override // cn.com.heaton.shiningmask.ui.utils.SoftKeyboardStateHelper.SoftKeyboardStateListener
            public void onSoftKeyboardOpened(int i2) {
                LogUtil.d("键盘弹出");
            }

            @Override // cn.com.heaton.shiningmask.ui.utils.SoftKeyboardStateHelper.SoftKeyboardStateListener
            public void onSoftKeyboardClosed() {
                LogUtil.d("键盘收起");
                TextEditActivity.this.hideKey();
            }
        });
        getDbData();
    }

    private void getDbData() {
        HistoryData historyData;
        DaoSession daoSession = App.getDaoSession();
        this.daoSession = daoSession;
        HistoryDataDao historyDataDao = daoSession.getHistoryDataDao();
        this.historyDataDao = historyDataDao;
        List<HistoryData> list = historyDataDao.queryBuilder().list();
        if (list != null) {
            setListViewHeight(list.size());
            this.historyDataList.clear();
            for (int size = list.size() - 1; size >= 0 && this.historyDataList.size() < 10; size--) {
                this.historyDataList.add(list.get(size));
            }
        }
        this.historyListAdapter = new HistoryListAdapter(this.mActivity, this.historyDataList);
        getBinding().rvHistoryList.setAdapter((ListAdapter) this.historyListAdapter);
        LinkedList<HistoryData> linkedList = this.historyDataList;
        if (linkedList == null || linkedList.isEmpty() || (historyData = this.historyDataList.get(0)) == null) {
            return;
        }
        this.curLedData = historyData.getData();
        this.curDataColorArray = historyData.getColorArray();
        getBinding().ltvPreview.setTextData(historyData.getData(), historyData.convertArray(historyData.getColorList()), this.textColorSelectEnable, this.textColorBgSelectEnable, this.gradientMode, this.bgColorMode);
    }

    private int[] getTextIconData(int i) {
        TypedArray typedArrayObtainTypedArray = getResources().obtainTypedArray(i);
        int length = typedArrayObtainTypedArray.length();
        int[] iArr = new int[typedArrayObtainTypedArray.length()];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = typedArrayObtainTypedArray.getResourceId(i2, 0);
        }
        typedArrayObtainTypedArray.recycle();
        return iArr;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.bleManager = ConnectActivity.getBleManager();
        this.musicPlayer = ConnectActivity.getMusicPlayer();
        this.textAgreement = TextAgreement.getInstance();
        this.iconDataList = TextIconData.getIconArray();
        initLedView();
        initTextData();
        initColorPickView();
    }

    private void initColorPickView() {
        getBinding().colorPicker1.post(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.3
            @Override // java.lang.Runnable
            public void run() {
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker1.setOldCenterColor(TextEditActivity.this.curTextColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker1.setNewCenterColor(TextEditActivity.this.curTextColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker1.setStateAngle(TextEditActivity.this.curTextColor);
            }
        });
        getBinding().colorPicker2.post(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.4
            @Override // java.lang.Runnable
            public void run() {
                LogUtil.d("设置色环的颜色：" + TextEditActivity.this.curTextBgColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker2.setOldCenterColor(TextEditActivity.this.curTextBgColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker2.setNewCenterColor(TextEditActivity.this.curTextBgColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker1.setStateAngle(TextEditActivity.this.curTextBgColor);
            }
        });
        getBinding().colorPicker3.post(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.5
            @Override // java.lang.Runnable
            public void run() {
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker3.setStateAngle(TextEditActivity.this.curAddTextColor);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).colorPicker3.setNewCenterColor(TextEditActivity.this.curAddTextColor);
            }
        });
        getBinding().colorPicker1.setEnabled(false);
        getBinding().colorPicker2.setEnabled(false);
    }

    private void initLedView() {
        getBinding().ltvPreview.setPointMargin(0);
        getBinding().ltvPreview.removeAllViews();
        getBinding().ltvPreview.init(48, 16);
        getBinding().ltvPreview.setSelectedColor(this.curTextColor);
        getBinding().ltvPreview.setLayerType(1, null);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().viewTop.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.6
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                if (motionEvent.getAction() == 1) {
                    TextEditActivity.this.hideHistorical();
                }
                return true;
            }
        });
        this.textImageIconAdapter.setOniClickListener(new TextImageIconAdapter.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.7
            @Override // cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter.OnItemClickListener
            public void onClick(int i) {
                LogUtil.d("onItem:" + i);
                TextEditActivity.this.setIconData((byte[]) TextEditActivity.this.iconDataList.get(i));
            }
        });
        getBinding().rbTextSelect.setOnCheckedChangeListener(new MultiLineRadioGroup.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.8
            @Override // cn.com.heaton.shiningmask.ui.widget.MultiLineRadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(MultiLineRadioGroup multiLineRadioGroup, int i) {
                if (((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect1.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect2.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect3.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect4.isChecked()) {
                    LogUtil.d("onCheckedChanged:" + TextEditActivity.this.gradientMode);
                    if (i == R.id.rb_text_select1) {
                        TextEditActivity.this.gradientMode = 1;
                        TextEditActivity.this.isShowZhizhen(false);
                        TextEditActivity textEditActivity = TextEditActivity.this;
                        textEditActivity.initDataAnim(textEditActivity.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity2 = TextEditActivity.this;
                        textEditActivity2.sendDefaultMode(0, textEditActivity2.textColorSelectEnable);
                    } else if (i == R.id.rb_text_select2) {
                        TextEditActivity.this.gradientMode = 2;
                        TextEditActivity.this.isShowZhizhen(false);
                        TextEditActivity textEditActivity3 = TextEditActivity.this;
                        textEditActivity3.initDataAnim(textEditActivity3.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity4 = TextEditActivity.this;
                        textEditActivity4.sendDefaultMode(1, textEditActivity4.textColorSelectEnable);
                    } else if (i == R.id.rb_text_select3) {
                        TextEditActivity.this.gradientMode = 3;
                        TextEditActivity.this.isShowZhizhen(false);
                        TextEditActivity textEditActivity5 = TextEditActivity.this;
                        textEditActivity5.initDataAnim(textEditActivity5.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity6 = TextEditActivity.this;
                        textEditActivity6.sendDefaultMode(2, textEditActivity6.textColorSelectEnable);
                    } else if (i == R.id.rb_text_select4) {
                        TextEditActivity.this.gradientMode = 4;
                        TextEditActivity.this.isShowZhizhen(false);
                        TextEditActivity textEditActivity7 = TextEditActivity.this;
                        textEditActivity7.initDataAnim(textEditActivity7.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity8 = TextEditActivity.this;
                        textEditActivity8.sendDefaultMode(3, textEditActivity8.textColorSelectEnable);
                    }
                    LogUtil.d("gradientMode:::" + TextEditActivity.this.gradientMode);
                }
            }
        });
        getBinding().rgTextBgClolor.setOnCheckedChangeListener(new MultiLineRadioGroup.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.9
            @Override // cn.com.heaton.shiningmask.ui.widget.MultiLineRadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(MultiLineRadioGroup multiLineRadioGroup, int i) {
                if (((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect1.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect2.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect3.isChecked() || ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect4.isChecked()) {
                    LogUtil.d("===========");
                    if (i == R.id.rb_gradient_select1) {
                        TextEditActivity.this.bgColorMode = 1;
                        TextEditActivity.this.isShowZhizhen2(false);
                        TextEditActivity textEditActivity = TextEditActivity.this;
                        textEditActivity.initDataAnim(textEditActivity.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity2 = TextEditActivity.this;
                        textEditActivity2.sendDefaultMode(4, textEditActivity2.textColorBgSelectEnable);
                        return;
                    }
                    if (i == R.id.rb_gradient_select2) {
                        TextEditActivity.this.bgColorMode = 2;
                        TextEditActivity.this.isShowZhizhen2(false);
                        TextEditActivity textEditActivity3 = TextEditActivity.this;
                        textEditActivity3.initDataAnim(textEditActivity3.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity4 = TextEditActivity.this;
                        textEditActivity4.sendDefaultMode(5, textEditActivity4.textColorBgSelectEnable);
                        return;
                    }
                    if (i == R.id.rb_gradient_select3) {
                        TextEditActivity.this.bgColorMode = 3;
                        TextEditActivity.this.isShowZhizhen2(false);
                        TextEditActivity textEditActivity5 = TextEditActivity.this;
                        textEditActivity5.initDataAnim(textEditActivity5.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity6 = TextEditActivity.this;
                        textEditActivity6.sendDefaultMode(6, textEditActivity6.textColorBgSelectEnable);
                        return;
                    }
                    if (i == R.id.rb_gradient_select4) {
                        TextEditActivity.this.bgColorMode = 4;
                        TextEditActivity.this.isShowZhizhen2(false);
                        TextEditActivity textEditActivity7 = TextEditActivity.this;
                        textEditActivity7.initDataAnim(textEditActivity7.curLedData, TextEditActivity.this.curAnimType);
                        TextEditActivity textEditActivity8 = TextEditActivity.this;
                        textEditActivity8.sendDefaultMode(7, textEditActivity8.textColorBgSelectEnable);
                    }
                }
            }
        });
        getBinding().rvHistoryList.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.10
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                HistoryData historyData = (HistoryData) TextEditActivity.this.historyDataList.get(i);
                if (historyData != null) {
                    TextEditActivity.this.curLedData = historyData.getData();
                    TextEditActivity.this.curDataColorArray = historyData.convertArray(historyData.getColorList());
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ltvPreview.cancelTimerTask();
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ltvPreview.setTextData(TextEditActivity.this.curLedData, TextEditActivity.this.curDataColorArray, TextEditActivity.this.textColorSelectEnable, TextEditActivity.this.textColorBgSelectEnable, TextEditActivity.this.gradientMode, TextEditActivity.this.bgColorMode);
                }
            }
        });
        getBinding().rvHistoryList.setOnItemLongClickListener(new AdapterView.OnItemLongClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.11
            @Override // android.widget.AdapterView.OnItemLongClickListener
            public boolean onItemLongClick(AdapterView<?> adapterView, View view, int i, long j) {
                TextEditActivity.this.deleteText(i);
                return false;
            }
        });
        getBinding().sbMoveLight.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.12
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                TextEditActivity.this.curSpeed = i;
                DataManager.getInstance().setCurSpeed(TextEditActivity.this.curSpeed);
                List<BleDevice> deviceList = App.getAppData().getDeviceList();
                for (int i2 = 0; i2 < deviceList.size(); i2++) {
                    TextEditActivity.this.sendSpeed(deviceList.get(i2), TextEditActivity.this.curSpeed);
                }
            }
        });
        getBinding().etTextInput.setOnEditorActionListener(new TextView.OnEditorActionListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.13
            @Override // android.widget.TextView.OnEditorActionListener
            public boolean onEditorAction(TextView textView, int i, KeyEvent keyEvent) {
                if (i != 4 && i != 5 && i != 6 && (keyEvent == null || 66 != keyEvent.getKeyCode() || keyEvent.getAction() != 0)) {
                    return false;
                }
                TextEditActivity.this.inputEnter();
                TextEditActivity.this.hideKey();
                return false;
            }
        });
        getBinding().colorPicker1.setOnColorChangedListener(new ColorPicker.OnColorChangedListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.14
            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onAlphaChanged(int i) {
            }

            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onColorChanged(int i, float f) {
                if (i == -1) {
                    return;
                }
                if (TextEditActivity.this.textColorSelectEnable) {
                    TextEditActivity.this.isShowZhizhen(true);
                } else {
                    TextEditActivity.this.isShowZhizhen(false);
                }
                TextEditActivity.this.curTextColor = i;
                float f2 = f + 90.0f;
                TextEditActivity.this.curTextAngle = f2;
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ivZhizhen1.setRotation(f2);
                LogUtil.d("角度：curTextAngle  " + f2 + " color:" + i);
                TextEditActivity.this.curTextColorR = Color.red(i);
                TextEditActivity.this.curTextColorG = Color.green(i);
                TextEditActivity.this.curTextColorB = Color.blue(i);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ltvPreview.setSelectedColor(i);
                TextEditActivity.this.gradientMode = 0;
                TextEditActivity textEditActivity = TextEditActivity.this;
                textEditActivity.initDataAnim(textEditActivity.curLedData, TextEditActivity.this.curAnimType);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect.clearCheck();
                TextEditActivity.this.sendTextColor((byte) Color.red(i), (byte) Color.green(i), (byte) Color.blue(i), TextEditActivity.this.textColorSelectEnable);
            }
        });
        getBinding().colorPicker2.setOnColorChangedListener(new ColorPicker.OnColorChangedListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.15
            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onAlphaChanged(int i) {
            }

            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onColorChanged(int i, float f) {
                if (i == -1) {
                    return;
                }
                float f2 = f + 90.0f;
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ivZhizhen2.setRotation(f2);
                LogUtil.d("角度2：curTextBgAngle  " + f2 + " color:" + i);
                TextEditActivity.this.curTextBgAngle = f2;
                TextEditActivity textEditActivity = TextEditActivity.this;
                textEditActivity.isShowZhizhen2(textEditActivity.textColorBgSelectEnable);
                double dRed = ((double) Color.red(i)) * 0.5d;
                double dGreen = ((double) Color.green(i)) * 0.5d;
                double dBlue = ((double) Color.blue(i)) * 0.5d;
                LogUtil.d("转换后：" + dRed + " " + dGreen + " " + dBlue);
                TextEditActivity.this.curTextBgColorR = (int) dRed;
                TextEditActivity.this.curTextBgColorG = (int) dGreen;
                TextEditActivity.this.curTextBgColorB = (int) dBlue;
                TextEditActivity.this.bgColorMode = 0;
                TextEditActivity textEditActivity2 = TextEditActivity.this;
                textEditActivity2.curTextBgColor = Color.rgb(textEditActivity2.curTextBgColorR, TextEditActivity.this.curTextBgColorG, TextEditActivity.this.curTextBgColorB);
                TextEditActivity textEditActivity3 = TextEditActivity.this;
                textEditActivity3.setTextLedViewBgColor(textEditActivity3.curTextBgColor);
                TextEditActivity textEditActivity4 = TextEditActivity.this;
                textEditActivity4.initDataAnim(textEditActivity4.curLedData, TextEditActivity.this.curAnimType);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rgTextBgClolor.clearCheck();
                TextEditActivity textEditActivity5 = TextEditActivity.this;
                textEditActivity5.sendTextBgColor((byte) textEditActivity5.curTextBgColorR, (byte) TextEditActivity.this.curTextBgColorG, (byte) TextEditActivity.this.curTextBgColorB, TextEditActivity.this.textColorBgSelectEnable);
            }
        });
        getBinding().colorPicker3.setOnColorChangedListener(new ColorPicker.OnColorChangedListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.16
            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onAlphaChanged(int i) {
            }

            @Override // cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker.OnColorChangedListener
            public void onColorChanged(int i, float f) {
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).ivZhizhen3.setRotation(f + 90.0f);
                ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).circleColor.setColor(i);
                TextEditActivity.this.curAddTextColor = i;
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setTextLedViewBgColor(int i) {
        LogUtil.d("设置TextLed背景颜色：" + this.textColorBgSelectEnable + " color:" + i);
        if (this.textColorBgSelectEnable) {
            getBinding().ltvPreview.setBgColor(i);
        } else {
            getBinding().ltvPreview.setBgColor(0);
        }
    }

    private void startLeftAnim() {
        clearAminMode();
        getBinding().ivAminMode.setImageResource(R.drawable.anim_left);
        AnimationDrawable animationDrawable = (AnimationDrawable) getBinding().ivAminMode.getDrawable();
        this.animMode = animationDrawable;
        animationDrawable.start();
    }

    private void startRightAnim() {
        clearAminMode();
        getBinding().ivAminMode.setImageResource(R.drawable.anim_right);
        AnimationDrawable animationDrawable = (AnimationDrawable) getBinding().ivAminMode.getDrawable();
        this.animMode = animationDrawable;
        animationDrawable.start();
    }

    private void startBlinkAnim() {
        clearAminMode();
        getBinding().ivAminMode.setImageResource(R.mipmap.anim_magic_blink_1);
        AlphaAnimation alphaAnimation = new AlphaAnimation(0.1f, 1.0f);
        alphaAnimation.setDuration(800L);
        alphaAnimation.setRepeatCount(-1);
        alphaAnimation.setRepeatMode(2);
        getBinding().ivAminMode.startAnimation(alphaAnimation);
    }

    private void startImmobilizationAnim() {
        clearAminMode();
        getBinding().ivAminMode.setImageResource(R.mipmap.anim_immobilization);
    }

    private void clearAminMode() {
        AnimationDrawable animationDrawable = this.animMode;
        if (animationDrawable != null) {
            animationDrawable.stop();
            getBinding().ivAminMode.clearAnimation();
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == R.id.iv_back) {
            if (this.isShowAddText) {
                hideAddText();
                hideKey();
                this.textList.clear();
                getBinding().rvLedviewList.removeAllViews();
                return;
            }
            finish();
            return;
        }
        if (id == R.id.iv_forward) {
            List<TextData> list = this.textList;
            if (list == null || list.isEmpty()) {
                return;
            }
            SoundManager.getInstance().textDelete();
            List<TextData> list2 = this.textList;
            list2.remove(list2.size() - 1);
            this.ledViewAdapter.setList(this.textList);
            this.ledViewAdapter.notifyDataSetChanged();
            viewLedUiUpdate();
            return;
        }
        if (id == R.id.iv_forward_1) {
            if (this.isShowHistorical) {
                hideHistorical();
                return;
            } else {
                showHistorical();
                return;
            }
        }
        if (id == R.id.iv_mode_previous) {
            int i = this.curAminMode;
            if (i >= 1) {
                this.curAminMode = i - 1;
            } else {
                this.curAminMode = 3;
            }
            aminSwitchover(this.curAminMode);
            sendTextModeCommand();
            return;
        }
        if (id == R.id.iv_mode_next) {
            int i2 = this.curAminMode;
            if (i2 <= 2) {
                this.curAminMode = i2 + 1;
            } else {
                this.curAminMode = 0;
            }
            aminSwitchover(this.curAminMode);
            sendTextModeCommand();
            return;
        }
        if (id == R.id.ibtn_send) {
            SoundManager.getInstance().textSend();
            saveLedData(this.curLedData, this.curDataColorArray);
            sendCotent(this.curLedData, this.curDataColorArray);
            return;
        }
        if (id == R.id.ll_text_edit) {
            return;
        }
        if (id == R.id.ll_ledView_preview) {
            hideHistorical();
            showAddText();
            showKey();
            return;
        }
        if (id == R.id.iv_ok) {
            getBinding().ltvPreview.cancelTimerTask();
            hideKey();
            List<TextData> list3 = this.textList;
            if (list3 == null || list3.isEmpty()) {
                return;
            }
            ArrayList arrayList = new ArrayList();
            for (int i3 = 0; i3 < this.textList.size(); i3++) {
                byte[] data = this.textList.get(i3).getData();
                if (data != null) {
                    for (byte b : data) {
                        arrayList.add(Byte.valueOf(b));
                    }
                }
            }
            byte[] bArr = new byte[arrayList.size()];
            for (int i4 = 0; i4 < arrayList.size(); i4++) {
                bArr[i4] = ((Byte) arrayList.get(i4)).byteValue();
            }
            ArrayList arrayList2 = new ArrayList();
            for (int i5 = 0; i5 < this.textList.size(); i5++) {
                int color = this.textList.get(i5).getColor();
                int widthCount = this.textList.get(i5).getWidthCount();
                LogUtil.d("color:" + color + " count:" + widthCount);
                for (int i6 = 0; i6 < widthCount; i6++) {
                    arrayList2.add(Integer.valueOf(color));
                }
            }
            int[] iArr = new int[arrayList2.size()];
            for (int i7 = 0; i7 < arrayList2.size(); i7++) {
                iArr[i7] = ((Integer) arrayList2.get(i7)).intValue();
            }
            this.curLedData = bArr;
            this.curDataColorArray = iArr;
            hideAddText();
            hideKey();
            this.textList.clear();
            getBinding().rvLedviewList.removeAllViews();
            initDataAnim(this.curLedData, this.curAnimType);
            return;
        }
        if (id == R.id.iv_go) {
            inputEnter();
            return;
        }
        if (id == R.id.view_led) {
            showKey();
            return;
        }
        if (id == R.id.view_text_pickcolor1) {
            LogUtil.d("点了view_text_pickcolor1:" + this.curTextBgColorR + "\u3000textColorSelectEnable：" + this.textColorSelectEnable);
            if (this.textColorSelectEnable) {
                hideColorSelect();
                senTextColorEnable(this.textColorSelectEnable);
            } else {
                showColorSelect();
                senTextColorEnable(this.textColorSelectEnable);
            }
            DataManager.getInstance().setTextColorEnable(this.textColorSelectEnable);
            sendTextColorMode();
            return;
        }
        if (id == R.id.view_text_pickcolor2) {
            LogUtil.d("点了view_text_pickcolor2:" + this.curTextBgColorR + "\u3000textColorSelectEnable：" + this.textColorBgSelectEnable);
            if (this.textColorBgSelectEnable) {
                hideTextBgColorSelect();
                setTextLedViewBgColor(0);
                initDataAnim(this.curLedData, this.curAnimType);
            } else {
                showTextBgColorSelect();
                setTextLedViewBgColor(this.curTextBgColor);
                initDataAnim(this.curLedData, this.curAnimType);
            }
            DataManager.getInstance().setTextColorBgEnable(this.textColorBgSelectEnable);
            sendTextBgMode();
        }
    }

    private void sendTextColorMode() {
        int i = this.gradientMode;
        if (i == 0) {
            LogUtil.d("sendTextColorMode.....");
            sendTextColor((byte) this.curTextColorR, (byte) this.curTextColorG, (byte) this.curTextColorB, this.textColorSelectEnable);
            return;
        }
        if (i == 1) {
            sendDefaultMode(0, this.textColorSelectEnable);
            return;
        }
        if (i == 2) {
            sendDefaultMode(1, this.textColorSelectEnable);
        } else if (i == 3) {
            sendDefaultMode(2, this.textColorSelectEnable);
        } else {
            if (i != 4) {
                return;
            }
            sendDefaultMode(3, this.textColorSelectEnable);
        }
    }

    private void sendTextBgMode() {
        LogUtil.d("bgColorMode:" + this.bgColorMode);
        int i = this.bgColorMode;
        if (i == 0) {
            sendTextBgColor((byte) this.curTextBgColorR, (byte) this.curTextBgColorG, (byte) this.curTextBgColorB, this.textColorBgSelectEnable);
            return;
        }
        if (i == 1) {
            sendDefaultMode(4, this.textColorBgSelectEnable);
            return;
        }
        if (i == 2) {
            sendDefaultMode(5, this.textColorBgSelectEnable);
        } else if (i == 3) {
            sendDefaultMode(6, this.textColorBgSelectEnable);
        } else {
            if (i != 4) {
                return;
            }
            sendDefaultMode(7, this.textColorBgSelectEnable);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideColorSelect() {
        this.textColorSelectEnable = false;
        getBinding().colorPicker1.setEnabled(false);
        getBinding().llColorUnselected.setVisibility(0);
        getBinding().rbTextSelect.setVisibility(8);
        getBinding().ivOffLight.setImageResource(R.mipmap.pickview_color_bg_unselected);
        getBinding().ivCenterSelectBottom.setImageResource(R.mipmap.pick_off_bg);
        getBinding().ivZhizhen1.setVisibility(8);
        textColorEnable(this.textColorSelectEnable);
        isShowZhizhen(false);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showColorSelect() {
        this.textColorSelectEnable = true;
        getBinding().colorPicker1.setEnabled(true);
        getBinding().llColorUnselected.setVisibility(8);
        getBinding().rbTextSelect.setVisibility(0);
        getBinding().ivOffLight.setImageResource(R.mipmap.pickview_color_bg_selected);
        getBinding().ivCenterSelectBottom.setImageResource(R.mipmap.pick_on_bg);
        textColorEnable(this.textColorSelectEnable);
        isShowZhizhen(this.gradientMode <= 0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void isShowZhizhen(boolean z) {
        if (z) {
            getBinding().ivZhizhen1.setVisibility(0);
        } else {
            getBinding().ivZhizhen1.setVisibility(8);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void isShowZhizhen2(boolean z) {
        if (z) {
            getBinding().ivZhizhen2.setVisibility(0);
        } else {
            getBinding().ivZhizhen2.setVisibility(8);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideTextBgColorSelect() {
        this.textColorBgSelectEnable = false;
        getBinding().colorPicker2.setEnabled(false);
        getBinding().llColorBgUnselected.setVisibility(0);
        getBinding().rgTextBgClolor.setVisibility(8);
        getBinding().ivZhizhen2.setVisibility(8);
        getBinding().ivOffLightBg.setImageResource(R.mipmap.pickview_color_bg_unselected);
        getBinding().ivCenterSelectBgBottom.setImageResource(R.mipmap.pick_off_bg);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showTextBgColorSelect() {
        this.textColorBgSelectEnable = true;
        getBinding().colorPicker2.setEnabled(true);
        getBinding().llColorBgUnselected.setVisibility(8);
        getBinding().rgTextBgClolor.setVisibility(0);
        getBinding().ivZhizhen2.setVisibility(0);
        getBinding().ivOffLightBg.setImageResource(R.mipmap.pickview_color_bg_selected);
        getBinding().ivCenterSelectBgBottom.setImageResource(R.mipmap.pick_on_bg);
        isShowZhizhen2(this.bgColorMode <= 0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void inputEnter() {
        setText(getBinding().etTextInput.getText().toString());
        getBinding().etTextInput.setText("");
        viewLedUiUpdate();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideHistorical() {
        this.isShowHistorical = false;
        getBinding().rvHistoryList.setVisibility(8);
        getBinding().top.ivForward1.setImageResource(R.mipmap.text_history_icon);
        getBinding().viewTop.setVisibility(8);
    }

    private void showHistorical() {
        this.isShowHistorical = true;
        getBinding().rvHistoryList.setVisibility(0);
        getBinding().top.ivForward1.setImageResource(R.mipmap.text_history_close);
        getBinding().viewTop.setVisibility(0);
    }

    private void showAddText() {
        this.isShowAddText = true;
        getBinding().top.ivForward1.setVisibility(8);
        getBinding().llTextPreview.setVisibility(8);
        getBinding().ibtnSend.setVisibility(8);
        getBinding().llTextAdd.setVisibility(0);
        getBinding().llLedViewPreview.setVisibility(8);
        getBinding().rvLedviewList.setVisibility(0);
        getBinding().viewLed.setVisibility(0);
        getBinding().top.ivForward.setImageResource(R.mipmap.text_magic_delete);
        getBinding().top.ivForward.setVisibility(0);
        getBinding().llBottom.setVisibility(0);
    }

    private void hideAddText() {
        this.isShowAddText = false;
        getBinding().top.ivForward1.setVisibility(0);
        getBinding().llTextAdd.setVisibility(8);
        getBinding().llTextPreview.setVisibility(0);
        getBinding().ibtnSend.setVisibility(0);
        getBinding().llLedViewPreview.setVisibility(0);
        getBinding().rvLedviewList.setVisibility(8);
        getBinding().viewLed.setVisibility(8);
        getBinding().top.ivForward.setVisibility(8);
        getBinding().llBottom.setVisibility(8);
    }

    private void viewLedUiUpdate() {
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < this.textList.size(); i++) {
            byte[] data = this.textList.get(i).getData();
            if (data != null) {
                for (byte b : data) {
                    arrayList.add(Byte.valueOf(b));
                }
            }
        }
        if (arrayList.size() > 96) {
            getBinding().viewLed.setVisibility(8);
        } else {
            getBinding().viewLed.setVisibility(0);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void aminSwitchover(int i) {
        this.curAminMode = i;
        DataManager.getInstance().setCurTextMode(i);
        if (i == 1) {
            this.curAnimType = 1;
            startLeftAnim();
        } else if (i == 2) {
            this.curAnimType = 2;
            startRightAnim();
        } else if (i == 3) {
            this.curAnimType = 3;
            startBlinkAnim();
        } else {
            this.curAnimType = 0;
            startImmobilizationAnim();
        }
    }

    private void sendTextModeCommand() {
        getBinding().ltvPreview.cancelTimerTask();
        this.mHandler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.17
            @Override // java.lang.Runnable
            public void run() {
                TextEditActivity textEditActivity = TextEditActivity.this;
                textEditActivity.initDataAnim(textEditActivity.curLedData, TextEditActivity.this.curAnimType);
            }
        }, 160L);
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i = 0; i < deviceList.size(); i++) {
            sendMode(deviceList.get(i), this.curAnimType);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void initDataAnim(byte[] bArr, int i) {
        DataManager.getInstance().setTextColorMode(this.gradientMode);
        DataManager.getInstance().setTextColorBgMode(this.bgColorMode);
        getBinding().ltvPreview.cancelTimerTask();
        LogUtil.d("初始数据动画：" + i + " gradientMode:" + this.gradientMode + " bgColorMode:" + this.bgColorMode);
        if (i == 1) {
            getBinding().ltvPreview.clearAnimation();
            getBinding().ltvPreview.setTextMarquee(bArr, this.curDataColorArray, this.textColorSelectEnable, this.textColorBgSelectEnable, this.gradientMode, this.bgColorMode);
            return;
        }
        if (i == 2) {
            getBinding().ltvPreview.clearAnimation();
            getBinding().ltvPreview.setTextRight(bArr, this.curDataColorArray, this.textColorSelectEnable, this.textColorBgSelectEnable, this.gradientMode, this.bgColorMode);
        } else {
            if (i == 3) {
                breatheAnim();
                getBinding().ltvPreview.setTextData(bArr, this.curDataColorArray, this.textColorSelectEnable, this.textColorBgSelectEnable, this.gradientMode, this.bgColorMode);
                return;
            }
            clearAminMode();
            this.curAminMode = 0;
            this.curAnimType = 0;
            getBinding().ltvPreview.clearAnimation();
            getBinding().ltvPreview.setTextData(bArr, this.curDataColorArray, this.textColorSelectEnable, this.textColorBgSelectEnable, this.gradientMode, this.bgColorMode);
        }
    }

    private void breatheAnim() {
        AlphaAnimation alphaAnimation = new AlphaAnimation(0.3f, 1.0f);
        alphaAnimation.setDuration(800L);
        alphaAnimation.setRepeatCount(-1);
        alphaAnimation.setRepeatMode(2);
        getBinding().ltvPreview.startAnimation(alphaAnimation);
    }

    private void setText(String str) {
        try {
            if (!TextUtils.isEmpty(str) && str.length() > 0) {
                for (char c : str.toCharArray()) {
                    String strValueOf = String.valueOf(c);
                    byte[] stringBytes = Text1664Bold.getStringBytes(strValueOf);
                    LogUtil.d("输入的文本：" + strValueOf + " 长度：" + stringBytes.length);
                    LogUtil.d("输入的文本data:" + ByteUtils.binaryToHexString(stringBytes));
                    TextData textData = new TextData();
                    textData.setType(0);
                    textData.setData(stringBytes);
                    textData.setColor(this.curAddTextColor);
                    textData.setWidthCount(stringBytes.length / 2);
                    textData.setContent(strValueOf);
                    this.textList.add(textData);
                }
                updateLedView(this.textList);
            }
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setIconData(byte[] bArr) {
        TextData textData = new TextData();
        textData.setType(1);
        textData.setData(bArr);
        textData.setWidthCount(16);
        textData.setColor(this.curAddTextColor);
        this.textList.add(textData);
        updateLedView(this.textList);
    }

    private void updateLedView(List<TextData> list) {
        this.ledViewAdapter.setList(list);
        this.ledViewAdapter.notifyDataSetChanged();
        getBinding().rvLedviewList.scrollToPosition(this.ledViewAdapter.getItemCount() - 1);
        viewLedUiUpdate();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showKey() {
        getBinding().llBottom.setVisibility(0);
        showPopupWindow();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideKey() {
        getBinding().llBottom.setVisibility(8);
        getBinding().etTextInput.setCursorVisible(true);
        hideSoftInput();
    }

    public void showPopupWindow() {
        getBinding().etTextInput.requestFocus();
        ((InputMethodManager) getSystemService("input_method")).showSoftInput(getBinding().etTextInput, 1);
    }

    public void hideSoftInput() {
        getBinding().etTextInput.clearFocus();
        ((InputMethodManager) getSystemService("input_method")).hideSoftInputFromWindow(getBinding().etTextInput.getWindowToken(), 2);
    }

    private void saveLedData(byte[] bArr, int[] iArr) {
        for (int i = 0; i < this.historyDataList.size(); i++) {
            if (Arrays.equals(this.historyDataList.get(i).getData(), bArr)) {
                return;
            }
        }
        HistoryData historyData = new HistoryData();
        historyData.setData(bArr);
        historyData.setColorList(iArr);
        this.historyDataList.addFirst(historyData);
        if (this.historyDataList.size() > 10) {
            deleteData(this.historyDataList.size() - 1);
        }
        App.getDaoSession().getHistoryDataDao().insert(historyData);
        setListViewHeight(this.historyDataList.size());
        this.historyListAdapter.setList(this.historyDataList);
        this.historyListAdapter.notifyDataSetChanged();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void deleteText(final int i) {
        new AlertDialog.Builder(this).setTitle(getString(R.string.reminder)).setMessage(R.string.delete_tip).setNegativeButton(getString(R.string.btn_cancel), new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.19
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i2) {
                dialogInterface.dismiss();
            }
        }).setPositiveButton(getString(com.cdbwsoft.library.R.string.btn_sure), new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.18
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i2) {
                TextEditActivity.this.deleteData(i);
                dialogInterface.dismiss();
            }
        }).create().show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void deleteData(int i) {
        try {
            this.historyListAdapter.setList(this.historyDataList);
            this.historyListAdapter.notifyDataSetChanged();
            this.historyDataDao.delete(this.historyDataList.get(i));
            setListViewHeight(this.historyDataList.size());
            this.historyDataList.remove(i);
            if (this.historyDataList.isEmpty()) {
                getBinding().ltvPreview.cancelTimerTask();
                getBinding().ltvPreview.clearSelected();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private void setListViewHeight(int i) {
        if (i > 4) {
            ViewGroup.LayoutParams layoutParams = getBinding().rvHistoryList.getLayoutParams();
            layoutParams.height = (int) DensityUtil.dp2px(this.mContext, 155.0f);
            getBinding().rvHistoryList.setLayoutParams(layoutParams);
        } else {
            ViewGroup.LayoutParams layoutParams2 = getBinding().rvHistoryList.getLayoutParams();
            layoutParams2.height = -2;
            getBinding().rvHistoryList.setLayoutParams(layoutParams2);
        }
    }

    public void sendSpeed(BleDevice bleDevice, int i) {
        LogUtil.d("发送速度命令");
        MusicPlayer musicPlayer = this.musicPlayer;
        if ((musicPlayer != null && musicPlayer.isPlaying()) || this.bleManager == null) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        if (i == 0) {
            i = 1;
        }
        byte[] speed = Agreement.getSpeed(i);
        LogUtil.e("sendSpeed:" + ((int) speed[6]));
        bleDevice.writeCharacteristic(Agreement.getEncryptData(speed));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendMode(BleDevice bleDevice, int i) {
        int i2 = 1;
        if (i != 0) {
            if (i == 1) {
                i2 = 3;
            } else if (i == 2) {
                i2 = 4;
            } else if (i == 3) {
                i2 = 2;
            }
        }
        sendCommand(bleDevice, i2);
    }

    public void sendCommand(BleDevice bleDevice, int i) {
        if (this.bleManager == null) {
            return;
        }
        MusicPlayer musicPlayer = this.musicPlayer;
        if (musicPlayer != null && musicPlayer.isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        bleDevice.writeCharacteristic(Agreement.getEncryptData(Agreement.getContentCommand(i)));
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendDefaultMode(int i, boolean z) {
        LogUtil.d("发送命令：" + z);
        if (this.bleManager == null) {
            return;
        }
        MusicPlayer musicPlayer = this.musicPlayer;
        if (musicPlayer != null && musicPlayer.isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        byte[] encryptData = Agreement.getEncryptData(Agreement.getDefaultMode(i, z));
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i2 = 0; i2 < deviceList.size(); i2++) {
            deviceList.get(i2).writeCharacteristic(encryptData);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendTextColor(byte b, byte b2, byte b3, boolean z) {
        LogUtil.d("发送文本颜色命令");
        if (this.bleManager == null) {
            return;
        }
        MusicPlayer musicPlayer = this.musicPlayer;
        if (musicPlayer != null && musicPlayer.isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        byte[] encryptData = Agreement.getEncryptData(Agreement.getTextColor(b, b2, b3, z));
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i = 0; i < deviceList.size(); i++) {
            deviceList.get(i).writeCharacteristic(encryptData);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendTextBgColor(byte b, byte b2, byte b3, boolean z) {
        LogUtil.d("发送文本背景颜色命令");
        MusicPlayer musicPlayer = this.musicPlayer;
        if (musicPlayer != null && musicPlayer.isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        byte[] encryptData = Agreement.getEncryptData(Agreement.getTextBgColor(b, b2, b3, z));
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i = 0; i < deviceList.size(); i++) {
            deviceList.get(i).writeCharacteristic(encryptData);
        }
    }

    public void sendCotent(byte[] bArr, int[] iArr) {
        LogUtil.d("发送文本数据：" + this.deviceList.size());
        if (this.bleManager == null) {
            return;
        }
        if (this.musicPlayer.isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        this.deviceList = deviceList;
        if (deviceList == null || deviceList.isEmpty()) {
            return;
        }
        TextAgreement.TextAgreementListener textAgreementListener = new TextAgreement.TextAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.20
            @Override // cn.com.heaton.shiningmask.model.data.TextAgreement.TextAgreementListener
            public void onFinishSend(final BleDevice bleDevice) {
                TextEditActivity textEditActivity = TextEditActivity.this;
                textEditActivity.sendMode(bleDevice, textEditActivity.curAnimType);
                TextEditActivity.this.mHandler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.20.1
                    @Override // java.lang.Runnable
                    public void run() {
                        TextEditActivity.this.sendSpeed(bleDevice, TextEditActivity.this.curSpeed);
                    }
                }, 150L);
                TextEditActivity.this.dismissProgressDialog();
            }
        };
        showProgressDialog(this.mActivity, getString(R.string.send));
        for (int i = 0; i < this.deviceList.size(); i++) {
            this.textAgreement.sendTextTo1236(this.deviceList.get(i), bArr, iArr, textAgreementListener);
        }
    }

    private void initTextData() {
        this.textColorSelectEnable = DataManager.getInstance().isTextColorEnable();
        this.textColorBgSelectEnable = DataManager.getInstance().isTextColorBgEnable();
        final int textColorMode = DataManager.getInstance().getTextColorMode();
        final int textColorBgMode = DataManager.getInstance().getTextColorBgMode();
        int curSpeed = DataManager.getInstance().getCurSpeed();
        final int curTextMode = DataManager.getInstance().getCurTextMode();
        this.curTextAngle = DataManager.getInstance().getTextColorAngle();
        this.curTextBgAngle = DataManager.getInstance().getTextColorBgAngle();
        this.curAminMode = curTextMode;
        this.curAnimType = curTextMode;
        this.curSpeed = curSpeed;
        LogUtil.d("curSpeed:" + curSpeed + " curTextMode:" + curTextMode + " textColorMode:" + textColorMode + " textColorBgMode:" + textColorBgMode + " textColorBgEnable:" + this.textColorBgSelectEnable + " textColorEnable:" + this.textColorSelectEnable);
        getBinding().sbMoveLight.setProgress(curSpeed);
        getBinding().ivZhizhen1.setRotation(this.curTextAngle);
        getBinding().ivZhizhen2.setRotation(this.curTextBgAngle);
        setTextLedViewBgColor(this.curTextBgColor);
        this.mHandler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.TextEditActivity.21
            @Override // java.lang.Runnable
            public void run() {
                int i = textColorMode;
                if (i == 1) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect1.setChecked(true);
                } else if (i == 2) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect2.setChecked(true);
                } else if (i == 3) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect3.setChecked(true);
                } else if (i == 4) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbTextSelect4.setChecked(true);
                }
                int i2 = textColorBgMode;
                if (i2 == 1) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect1.setChecked(true);
                } else if (i2 == 2) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect2.setChecked(true);
                } else if (i2 == 3) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect3.setChecked(true);
                } else if (i2 == 4) {
                    ((ActivityTextEdit2Binding) TextEditActivity.this.getBinding()).rbGradientSelect4.setChecked(true);
                }
                TextEditActivity textEditActivity = TextEditActivity.this;
                textEditActivity.initDataAnim(textEditActivity.curLedData, curTextMode);
                TextEditActivity.this.aminSwitchover(curTextMode);
                if (TextEditActivity.this.textColorSelectEnable) {
                    TextEditActivity.this.showColorSelect();
                } else {
                    TextEditActivity.this.hideColorSelect();
                }
                if (TextEditActivity.this.textColorBgSelectEnable) {
                    TextEditActivity.this.showTextBgColorSelect();
                } else {
                    TextEditActivity.this.hideTextBgColorSelect();
                }
            }
        }, 150L);
    }

    private void textColorEnable(boolean z) {
        DataManager.getInstance().setTextColorEnable(z);
        initDataAnim(this.curLedData, this.curAnimType);
    }

    private void senTextColorEnable(boolean z) {
        int i = this.gradientMode;
        if (i == 0) {
            LogUtil.d("文本颜色开关UI显示");
            sendTextColor((byte) this.curTextColorR, (byte) this.curTextColorG, (byte) this.curTextColorB, z);
            return;
        }
        if (i == 1) {
            sendDefaultMode(0, z);
            return;
        }
        if (i == 2) {
            sendDefaultMode(1, z);
        } else if (i == 3) {
            sendDefaultMode(2, z);
        } else {
            if (i != 4) {
                return;
            }
            sendDefaultMode(3, z);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        clearAminMode();
        LogUtil.d("curAminMode：" + this.curAminMode + " curTextAngle:" + this.curTextAngle + " curTextColor:" + this.curTextColor + " curTextBgAngle:" + this.curTextBgAngle + "curTextBgColor:" + this.curTextBgColor);
        DataManager.getInstance().setCurTextMode(this.curAminMode);
        DataManager.getInstance().setTextColorAngle(this.curTextAngle);
        DataManager.getInstance().setTextColorBgAngle(this.curTextBgAngle);
        DataManager.getInstance().setTextColor(this.curTextColor);
        DataManager.getInstance().setTextBgColor(this.curTextBgColor);
    }
}
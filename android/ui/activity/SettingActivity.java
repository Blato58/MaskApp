package cn.com.heaton.shiningmask.ui.activity;

import android.content.Intent;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.RadioGroup;
import androidx.core.content.ContextCompat;
import cn.com.heaton.shiningmask.BuildConfig;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.AppManager;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.app.LanguageUtil;
import cn.com.heaton.shiningmask.base.app.SoundManager;
import cn.com.heaton.shiningmask.base.update.UpdateManager;
import cn.com.heaton.shiningmask.dao.DeviceDao;
import cn.com.heaton.shiningmask.dao.bean.Device;
import cn.com.heaton.shiningmask.databinding.ActivitySettingsBinding;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.model.data.DiyAgreement;
import cn.com.heaton.shiningmask.ui.utils.AppUtils;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import com.cdbwsoft.library.AppConfig;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleManager;
import java.util.ArrayList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class SettingActivity extends BaseActivity<ActivitySettingsBinding> implements View.OnClickListener {
    private BleManager bleManager;
    private int curSendIndex;
    byte[] dataByte;
    private int gestureEnable;
    private int imageSelectMode;
    private int loopMode;
    private String mLanguage;
    private List<List<Integer>> sendDataList;
    Handler handler = new Handler();
    private int sendIndex = 0;
    DiyAgreement.DiyAgreementListener diyAgreementListener = new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.7
        @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
        public void onFaceOk(BleDevice bleDevice) {
            LogUtil.d("onFaceOk>>>" + SettingActivity.this.curSendIndex + " sendDataList.size:0" + SettingActivity.this.sendDataList);
            SettingActivity.this.curSendIndex++;
            if (SettingActivity.this.curSendIndex < SettingActivity.this.sendDataList.size()) {
                SettingActivity.this.senData(bleDevice, ByteUtils.listConvertByteArray((List) SettingActivity.this.sendDataList.get(SettingActivity.this.curSendIndex)));
            } else {
                SettingActivity.this.curSendIndex = 0;
                new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.7.1
                    @Override // java.lang.Runnable
                    public void run() {
                        SettingActivity.this.dismissProgressDialog();
                    }
                }, 500L);
            }
        }
    };

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivitySettingsBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivitySettingsBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().top.ivForward.setOnClickListener(this);
        getBinding().aset.layoutChina.setOnClickListener(this);
        getBinding().aset.layoutEnglish.setOnClickListener(this);
        getBinding().aset.rltlaoZhTw.setOnClickListener(this);
        getBinding().aset.rltlaoJa.setOnClickListener(this);
        getBinding().aset.rltlaoDe.setOnClickListener(this);
        getBinding().aset.rltlaoPt.setOnClickListener(this);
        getBinding().aset.rltlaoEs.setOnClickListener(this);
        getBinding().aset.rltlaoFr.setOnClickListener(this);
        getBinding().aset.rltlaoKo.setOnClickListener(this);
        getBinding().aset.rltlaoRu.setOnClickListener(this);
        getBinding().llFinger.setOnClickListener(this);
        getBinding().llLanuages.setOnClickListener(this);
        getBinding().aset.llLanuage.setOnClickListener(this);
        getBinding().top.ivBack.setOnClickListener(this);
        getBinding().rbAll.setOnClickListener(this);
        getBinding().rbImage.setOnClickListener(this);
        getBinding().rbDiy.setOnClickListener(this);
        getBinding().rbOrder.setOnClickListener(this);
        getBinding().rbRandom.setOnClickListener(this);
        getBinding().top.tvTitle.setText(R.string.setting);
        getBinding().top.ivForward.setVisibility(8);
        getBinding().rbFinger.setChecked(true);
        getBinding().rbAll.setChecked(true);
        getBinding().rbOrder.setChecked(true);
        final String appMetaData = AppUtils.getAppMetaData(this.mActivity, AppConfig.META_CHANNEL);
        String str = appMetaData.equals(BuildConfig.FLAVOR) ? "G-V" : "S-V";
        String versionName = App.getInstance().getVersionName();
        getBinding().tvVersionSettings.setText(String.format("%s%s", str, versionName));
        LogUtil.d("当前版本号：" + versionName);
        if (!appMetaData.equals(BuildConfig.FLAVOR)) {
            getBinding().checkUp.setVisibility(0);
            setUpdateTips(UpdateManager.isNeedUpDateApp);
        } else {
            getBinding().checkUp.setVisibility(8);
        }
        getBinding().checkUp.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity$$ExternalSyntheticLambda0
            @Override // android.view.View.OnClickListener
            public final void onClick(View view) {
                this.f$0.lambda$initView$0(appMetaData, view);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public /* synthetic */ void lambda$initView$0(String str, View view) {
        if (!str.equals(BuildConfig.FLAVOR) && UpdateManager.isNeedUpDateApp) {
            new UpdateManager(this).versionUpdate();
        } else {
            ToastUtil.showToast(getString(R.string.is_new_version));
        }
    }

    private void setUpdateTips(final boolean z) {
        runOnUiThread(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.1
            @Override // java.lang.Runnable
            public void run() {
                if (z) {
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).checkUpText.setText(SettingActivity.this.getString(R.string.check_update));
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).checkUpText.setTextColor(ContextCompat.getColor(SettingActivity.this, R.color.update_version_color));
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).checkUpImage.setVisibility(0);
                    return;
                }
                ((ActivitySettingsBinding) SettingActivity.this.getBinding()).checkUpImage.setVisibility(8);
            }
        });
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.bleManager = ConnectActivity.getBleManager();
        this.gestureEnable = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_GESTURE_ENABLE, 0)).intValue();
        this.imageSelectMode = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, 0)).intValue();
        this.loopMode = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_LOOP_MODE, 0)).intValue();
        if (this.gestureEnable == 1) {
            getBinding().cbGesture.setChecked(true);
            getBinding().llSettings.setVisibility(0);
        } else {
            getBinding().cbGesture.setChecked(false);
            getBinding().llSettings.setVisibility(8);
        }
        LogUtil.d("gestureEnable:" + this.gestureEnable);
        LogUtil.d("imageSelectMode:" + this.imageSelectMode);
        LogUtil.d("loopMode:" + this.loopMode);
        int i = this.imageSelectMode;
        if (i == 1) {
            getBinding().rbImage.setChecked(true);
        } else if (i == 2) {
            getBinding().rbDiy.setChecked(true);
        } else {
            getBinding().rbAll.setChecked(true);
        }
        if (this.loopMode == 1) {
            getBinding().rbRandom.setChecked(true);
        } else {
            getBinding().rbOrder.setChecked(true);
        }
        this.mLanguage = LanguageUtil.getSaveLanguage(this);
        LogUtil.e("mLanguage:" + this.mLanguage);
        if (this.mLanguage.equals("zh")) {
            checkZh();
            return;
        }
        if (this.mLanguage.equals("en")) {
            checkEn();
            return;
        }
        if (this.mLanguage.equals("zh_TW")) {
            checkZhTw();
            return;
        }
        if (this.mLanguage.equals("ja")) {
            checkJa();
            return;
        }
        if (this.mLanguage.equals("de")) {
            checkDe();
            return;
        }
        if (this.mLanguage.equals("pt")) {
            checkPt();
            return;
        }
        if (this.mLanguage.equals("es")) {
            checkEs();
            return;
        }
        if (this.mLanguage.equals("fr")) {
            checkFr();
            return;
        }
        if (this.mLanguage.equals("ko")) {
            checkKo();
        } else if (this.mLanguage.equals("ru")) {
            checkRu();
        } else {
            checkEn();
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().rgMenu.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.2
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup, int i) {
                if (i == R.id.rb_finger) {
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llFinger.setVisibility(0);
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llLanuages.setVisibility(8);
                } else if (i == R.id.rb_languages) {
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llFinger.setVisibility(8);
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llLanuages.setVisibility(0);
                }
            }
        });
        getBinding().cbGesture.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.3
            @Override // android.widget.CompoundButton.OnCheckedChangeListener
            public void onCheckedChanged(CompoundButton compoundButton, boolean z) {
                if (z) {
                    SettingActivity.this.gestureEnable = 1;
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llSettings.setVisibility(0);
                } else {
                    SettingActivity.this.gestureEnable = 0;
                    ((ActivitySettingsBinding) SettingActivity.this.getBinding()).llSettings.setVisibility(8);
                }
                SPUtils.put(SettingActivity.this.mContext, C.SP.SETTINGS_GESTURE_ENABLE, Integer.valueOf(SettingActivity.this.gestureEnable));
                SettingActivity.this.sendSetData();
            }
        });
        getBinding().rgGallerySelection.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.4
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup, int i) {
                if (i == R.id.rb_all) {
                    SettingActivity.this.imageSelectMode = 0;
                    SPUtils.put(SettingActivity.this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, Integer.valueOf(SettingActivity.this.imageSelectMode));
                    SettingActivity.this.sendSetData();
                } else if (i == R.id.rb_image) {
                    SettingActivity.this.imageSelectMode = 1;
                    SPUtils.put(SettingActivity.this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, Integer.valueOf(SettingActivity.this.imageSelectMode));
                } else if (i == R.id.rb_diy) {
                    SettingActivity.this.imageSelectMode = 2;
                    SPUtils.put(SettingActivity.this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, Integer.valueOf(SettingActivity.this.imageSelectMode));
                }
                LogUtil.d("checkedId:" + SettingActivity.this.imageSelectMode);
            }
        });
        getBinding().rgLoop.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.5
            @Override // android.widget.RadioGroup.OnCheckedChangeListener
            public void onCheckedChanged(RadioGroup radioGroup, int i) {
                if (i == R.id.rb_order) {
                    SettingActivity.this.loopMode = 0;
                } else if (i == R.id.rb_random) {
                    SettingActivity.this.loopMode = 1;
                }
                SPUtils.put(SettingActivity.this.mContext, C.SP.SETTINGS_LOOP_MODE, Integer.valueOf(SettingActivity.this.loopMode));
                SettingActivity.this.sendSetData();
            }
        });
    }

    private void back() {
        SoundManager.getInstance().textBack();
        if (isChangedLanguage()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY1);
            saveLanguage();
            updateDeviceListToDb();
            List<BleDevice> deviceList = App.getAppData().getDeviceList();
            if (deviceList != null) {
                for (int i = 0; i < deviceList.size(); i++) {
                    deviceList.get(i).disconnect();
                }
            }
            deviceList.clear();
            BleManager bleManager = this.bleManager;
            if (bleManager != null) {
                bleManager.release();
            }
            this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.SettingActivity.6
                @Override // java.lang.Runnable
                public void run() {
                    AppManager.getAppManager().finishAllActivity();
                    SettingActivity.this.startActivity(new Intent(SettingActivity.this, (Class<?>) ConnectActivity.class));
                    SettingActivity.this.finish();
                    SettingActivity.this.overridePendingTransition(R.anim.slide_left_in, R.anim.slide_right_out);
                }
            }, 500L);
            return;
        }
        finish();
        overridePendingTransition(R.anim.slide_left_in, R.anim.slide_right_out);
    }

    private void updateDeviceListToDb() {
        DeviceDao deviceDao = App.getDaoSession().getDeviceDao();
        List<Device> listLoadAll = deviceDao.loadAll();
        if (listLoadAll == null || listLoadAll.isEmpty()) {
            return;
        }
        for (int i = 0; i < listLoadAll.size(); i++) {
            listLoadAll.get(i).setIsReConnect(false);
        }
        deviceDao.updateInTx(listLoadAll);
    }

    private void checkZh() {
        this.mLanguage = "zh";
        getBinding().aset.ivChina.setVisibility(0);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkEn() {
        this.mLanguage = "en";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(0);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkZhTw() {
        this.mLanguage = "zh_TW";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(0);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkJa() {
        this.mLanguage = "ja";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(0);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkDe() {
        this.mLanguage = "de";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(0);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkEs() {
        this.mLanguage = "es";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(0);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkPt() {
        this.mLanguage = "pt";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(0);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkFr() {
        this.mLanguage = "fr";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(0);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkKo() {
        this.mLanguage = "ko";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(0);
        getBinding().aset.imgvRu.setVisibility(8);
    }

    private void checkRu() {
        this.mLanguage = "ru";
        getBinding().aset.ivChina.setVisibility(8);
        getBinding().aset.ivEnglish.setVisibility(8);
        getBinding().aset.imgvZhTw.setVisibility(8);
        getBinding().aset.imgvJa.setVisibility(8);
        getBinding().aset.imgvDe.setVisibility(8);
        getBinding().aset.imgvPt.setVisibility(8);
        getBinding().aset.imgvEs.setVisibility(8);
        getBinding().aset.imgvFr.setVisibility(8);
        getBinding().aset.imgvKo.setVisibility(8);
        getBinding().aset.imgvRu.setVisibility(0);
    }

    private String getLocalLanguage() {
        return getResources().getConfiguration().locale.getLanguage();
    }

    public void saveLanguage() {
        SPUtils.put(this, C.SP.LANGUAGE, this.mLanguage);
    }

    public boolean isChangedLanguage() {
        return !this.mLanguage.equalsIgnoreCase(LanguageUtil.getSaveLanguage(this));
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == R.id.iv_back) {
            back();
            return;
        }
        if (id == R.id.layout_china) {
            checkZh();
            return;
        }
        if (id == R.id.layout_english) {
            checkEn();
            return;
        }
        if (id == R.id.rltlao_zh_tw) {
            checkZhTw();
            return;
        }
        if (id == R.id.rltlao_ja) {
            checkJa();
            return;
        }
        if (id == R.id.rltlao_de) {
            checkDe();
            return;
        }
        if (id == R.id.rltlao_pt) {
            checkPt();
            return;
        }
        if (id == R.id.rltlao_es) {
            checkEs();
            return;
        }
        if (id == R.id.rltlao_fr) {
            checkFr();
            return;
        }
        if (id == R.id.rltlao_ko) {
            checkKo();
            return;
        }
        if (id == R.id.rltlao_ru) {
            checkRu();
            return;
        }
        if (id == R.id.ll_finger) {
            return;
        }
        if (id == R.id.rb_image) {
            toImageSelectActivity(0);
        } else if (id == R.id.rb_diy) {
            toImageSelectActivity(1);
        }
    }

    private void toImageSelectActivity(int i) {
        Intent intent = new Intent(this.mContext, (Class<?>) GallerySelectionActivity.class);
        intent.putExtra("flag", i);
        startActivity(intent);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendSetData() {
        if (this.bleManager == null) {
            return;
        }
        this.curSendIndex = 0;
        String str = (String) SPUtils.get(this.mContext, C.SP.SETTINGS_IMAGE_DATA, null);
        String str2 = (String) SPUtils.get(this.mContext, C.SP.SETTINGS_DIY_IMAGE_DATA, null);
        int iIntValue = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_LOOP_MODE, 0)).intValue();
        int iIntValue2 = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_GESTURE_ENABLE, 0)).intValue();
        LogUtil.d("获取到的变脸数据：imageSelectMode:" + this.imageSelectMode + " imageSelectMode");
        int i = this.imageSelectMode;
        if (i > 0) {
            if (i == 1) {
                this.sendDataList = getSendDataByeStr(str);
            } else if (i == 2) {
                this.sendDataList = getSendDataByeStr(str2);
            }
            List<List<Integer>> list = this.sendDataList;
            if (list != null && !list.isEmpty()) {
                LogUtil.d("获取到的变脸数据：sendDataList:" + this.sendDataList + " sendDataList.size:" + this.sendDataList.size());
                sendImageDataToDevice(this.dataByte.length, ByteUtils.listConvertByteArray(this.sendDataList.get(0)), iIntValue2, iIntValue, this.imageSelectMode);
                return;
            } else {
                LogUtil.d("发送");
                sendImageDataToDevice(0, null, iIntValue2, iIntValue, this.imageSelectMode);
                return;
            }
        }
        LogUtil.d("发送2");
        sendImageDataToDevice(0, null, iIntValue2, iIntValue, this.imageSelectMode);
    }

    private void sendImageDataToDevice(int i, byte[] bArr, int i2, int i3, int i4) {
        if (bArr == null) {
            bArr = new byte[0];
        }
        byte[] gestureSettings = Agreement.getGestureSettings((byte) i, bArr, (byte) i2, (byte) i3, (byte) i4);
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList == null || deviceList.isEmpty()) {
            return;
        }
        if (ConnectActivity.getMusicPlayer().isPlaying()) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        }
        for (int i5 = 0; i5 < deviceList.size(); i5++) {
            DiyAgreement.getInstance().sendDiyImageSetting(deviceList.get(i5), gestureSettings, this.diyAgreementListener);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void senData(BleDevice bleDevice, byte[] bArr) {
        if (bArr == null || bArr.length <= 0) {
            return;
        }
        DiyAgreement.getInstance().sendDiyImageSetting(bleDevice, Agreement.getGestureSettings2(bArr), this.diyAgreementListener);
    }

    private List<List<Integer>> getSendDataByeStr(String str) {
        ArrayList arrayList = new ArrayList();
        if (str != null) {
            ArrayList arrayList2 = new ArrayList();
            String[] strArrSplit = str.split(",");
            this.dataByte = new byte[strArrSplit.length];
            int i = 0;
            for (int i2 = 0; i2 < strArrSplit.length; i2++) {
                this.dataByte[i2] = Byte.parseByte(strArrSplit[i2]);
                arrayList2.add(Integer.valueOf(strArrSplit[i2]));
            }
            if (arrayList2.size() > 7) {
                ArrayList arrayList3 = new ArrayList();
                ArrayList arrayList4 = new ArrayList();
                for (int i3 = 0; i3 < 7; i3++) {
                    arrayList3.add((Integer) arrayList2.get(i3));
                }
                for (int i4 = 7; i4 < arrayList2.size(); i4++) {
                    arrayList4.add((Integer) arrayList2.get(i4));
                }
                arrayList.add(arrayList3);
                List listSplitList = ByteUtils.splitList(arrayList4);
                while (i < listSplitList.size()) {
                    arrayList.add((List) listSplitList.get(i));
                    i++;
                }
            } else {
                ArrayList arrayList5 = new ArrayList();
                while (i < arrayList2.size()) {
                    arrayList5.add((Integer) arrayList2.get(i));
                    i++;
                }
                arrayList.add(arrayList5);
            }
        }
        return arrayList;
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        super.onBackPressed();
        back();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        DiyAgreement.getInstance().clear();
    }
}
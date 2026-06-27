package cn.com.heaton.shiningmask.ui.activity;

import android.content.res.TypedArray;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.dao.DaoSession;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import cn.com.heaton.shiningmask.databinding.ActivityGallerySelectionBinding;
import cn.com.heaton.shiningmask.model.bean.DefaultImage;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.model.data.DiyAgreement;
import cn.com.heaton.shiningmask.ui.adapter.DefaultImageAllListAdapter;
import cn.com.heaton.shiningmask.ui.adapter.DiyImageAllListAdapter;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import com.cdbwsoft.library.ble.BleDevice;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Objects;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class GallerySelectionActivity extends BaseActivity<ActivityGallerySelectionBinding> implements View.OnClickListener {
    private int curIndex;
    private int curSendIndex;
    private DaoSession daoSession;
    private byte[] dataByte;
    DefaultImageAllListAdapter defaultImageAllListAdapter;
    DiyImageAllListAdapter diyImageAllListAdapter;
    private int flag;
    private int imageSelectMode;
    private boolean isAll;
    private List<List<Integer>> sendDataList;
    private List<CropImage> diyDataList = new ArrayList();
    private List<DefaultImage> defaultImageList = new ArrayList();
    DiyAgreement.DiyAgreementListener diyAgreementListener = new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.9
        @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
        public void onFaceOk(BleDevice bleDevice) {
            LogUtil.d("onFaceOk>>>" + GallerySelectionActivity.this.curSendIndex + " sendDataList.size:0" + GallerySelectionActivity.this.sendDataList);
            GallerySelectionActivity.this.curSendIndex++;
            if (GallerySelectionActivity.this.curSendIndex < GallerySelectionActivity.this.sendDataList.size()) {
                GallerySelectionActivity.this.senData(bleDevice, ByteUtils.listConvertByteArray((List) GallerySelectionActivity.this.sendDataList.get(GallerySelectionActivity.this.curSendIndex)));
            } else {
                GallerySelectionActivity.this.curSendIndex = 0;
                new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.9.1
                    @Override // java.lang.Runnable
                    public void run() {
                        GallerySelectionActivity.this.dismissProgressDialog();
                        if (((Integer) SPUtils.get(GallerySelectionActivity.this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, 0)).intValue() == 2) {
                            ToastUtil.showToast(GallerySelectionActivity.this.getString(R.string.settings_tips));
                        }
                        GallerySelectionActivity.this.finish();
                    }
                }, 500L);
            }
        }
    };

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityGallerySelectionBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityGallerySelectionBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivBack.setOnClickListener(this);
        getBinding().ivForward.setOnClickListener(this);
        getBinding().rlBottomMenu.setOnClickListener(this);
        getBinding().ivBack.setImageResource(R.mipmap.all_diy_close);
        getBinding().ivForward.setVisibility(0);
        getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        int intExtra = getIntent().getIntExtra("flag", 0);
        this.flag = intExtra;
        if (intExtra == 1) {
            getBinding().gvDefaultImage.setVisibility(8);
            getBinding().lvImage.setVisibility(0);
            initDiyImageData();
            initBottomStaus();
        } else {
            getBinding().gvDefaultImage.setVisibility(0);
            getBinding().lvImage.setVisibility(8);
            initDefaultImageData();
            initBottomStaus2();
        }
        setTvTitle();
    }

    private void initDiyImageData() {
        DaoSession daoSession = App.getDaoSession();
        this.daoSession = daoSession;
        this.diyDataList = daoSession.getCropImageDao().queryBuilder().list();
        clearSort();
        this.diyImageAllListAdapter = new DiyImageAllListAdapter(this.mContext, this.diyDataList);
        getBinding().lvImage.setAdapter((ListAdapter) this.diyImageAllListAdapter);
    }

    private void initDefaultImageData() {
        int[] defaultImageData;
        if (ConnectActivity.isShowImageFlag()) {
            defaultImageData = getDefaultImageData(R.array.image_default_new);
        } else {
            defaultImageData = getDefaultImageData(R.array.image_default);
        }
        LogUtil.d("expressionArray:" + defaultImageData.length);
        for (int i = 0; i < defaultImageData.length; i++) {
            DefaultImage defaultImage = new DefaultImage();
            defaultImage.setId(i);
            defaultImage.setImgRes(defaultImageData[i]);
            this.defaultImageList.add(defaultImage);
        }
        this.defaultImageAllListAdapter = new DefaultImageAllListAdapter(this.mContext, this.defaultImageList);
        getBinding().gvDefaultImage.setAdapter((ListAdapter) this.defaultImageAllListAdapter);
    }

    private int[] getDefaultImageData(int i) {
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
    protected void bindListener() {
        getBinding().lvImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.1
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                CropImage cropImage = (CropImage) GallerySelectionActivity.this.diyDataList.get(i);
                if (cropImage.getIndex() > 0) {
                    GallerySelectionActivity.this.isAll = false;
                    ((ActivityGallerySelectionBinding) GallerySelectionActivity.this.getBinding()).ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
                    cropImage.setIndex(0);
                    GallerySelectionActivity gallerySelectionActivity = GallerySelectionActivity.this;
                    gallerySelectionActivity.diyDataSort(gallerySelectionActivity.diyDataList);
                } else {
                    GallerySelectionActivity gallerySelectionActivity2 = GallerySelectionActivity.this;
                    int i2 = gallerySelectionActivity2.curIndex + 1;
                    gallerySelectionActivity2.curIndex = i2;
                    cropImage.setIndex(i2);
                }
                GallerySelectionActivity.this.initBottomStaus();
                GallerySelectionActivity.this.diyImageAllListAdapter.notifyDataSetChanged();
                GallerySelectionActivity.this.setTvTitle();
            }
        });
        getBinding().gvDefaultImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.2
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                DefaultImage defaultImage = (DefaultImage) GallerySelectionActivity.this.defaultImageList.get(i);
                if (defaultImage.getIndex() > 0) {
                    GallerySelectionActivity.this.isAll = false;
                    ((ActivityGallerySelectionBinding) GallerySelectionActivity.this.getBinding()).ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
                    defaultImage.setIndex(0);
                    for (int i2 = 0; i2 < GallerySelectionActivity.this.defaultImageList.size(); i2++) {
                        LogUtil.d("diyData:" + ((DefaultImage) GallerySelectionActivity.this.defaultImageList.get(i2)).toString());
                    }
                    GallerySelectionActivity gallerySelectionActivity = GallerySelectionActivity.this;
                    gallerySelectionActivity.defaultImageDataSort(gallerySelectionActivity.defaultImageList);
                } else {
                    GallerySelectionActivity gallerySelectionActivity2 = GallerySelectionActivity.this;
                    int i3 = gallerySelectionActivity2.curIndex + 1;
                    gallerySelectionActivity2.curIndex = i3;
                    defaultImage.setIndex(i3);
                }
                GallerySelectionActivity.this.initBottomStaus2();
                GallerySelectionActivity.this.setTvTitle();
                GallerySelectionActivity.this.defaultImageAllListAdapter.notifyDataSetChanged();
            }
        });
    }

    private void selectAllSort() {
        int i = 1;
        if (this.flag == 1) {
            while (i <= this.diyDataList.size()) {
                this.diyDataList.get(i - 1).setIndex(i);
                i++;
            }
        } else {
            while (i <= this.defaultImageList.size()) {
                this.defaultImageList.get(i - 1).setIndex(i);
                i++;
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void diyDataSort(List<CropImage> list) {
        Collections.sort(list, new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.3
            @Override // java.util.Comparator
            public int compare(CropImage cropImage, CropImage cropImage2) {
                if (cropImage.getIndex() > cropImage2.getIndex()) {
                    return 1;
                }
                return cropImage.getIndex() == cropImage2.getIndex() ? 0 : -1;
            }
        });
        int i = 0;
        for (int i2 = 0; i2 < list.size(); i2++) {
            if (list.get(i2).getIndex() != 0) {
                i++;
                list.get(i2).setIndex(i);
            }
        }
        this.curIndex = i;
        Collections.sort(list, new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.4
            @Override // java.util.Comparator
            public int compare(CropImage cropImage, CropImage cropImage2) {
                if (cropImage.getId().longValue() > cropImage2.getId().longValue()) {
                    return 1;
                }
                return cropImage.getId() == cropImage2.getId() ? 0 : -1;
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void defaultImageDataSort(List<DefaultImage> list) {
        list.sort(new Comparator<DefaultImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.5
            @Override // java.util.Comparator
            public int compare(DefaultImage defaultImage, DefaultImage defaultImage2) {
                return Integer.compare(defaultImage.getIndex(), defaultImage2.getIndex());
            }
        });
        int i = 0;
        for (int i2 = 0; i2 < list.size(); i2++) {
            if (list.get(i2).getIndex() != 0) {
                i++;
                list.get(i2).setIndex(i);
            }
        }
        this.curIndex = i;
        Collections.sort(list, new Comparator<DefaultImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.6
            @Override // java.util.Comparator
            public int compare(DefaultImage defaultImage, DefaultImage defaultImage2) {
                return Integer.compare(defaultImage.getId(), defaultImage2.getId());
            }
        });
    }

    private List<CropImage> getSelectDiyData() {
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < this.diyDataList.size(); i++) {
            CropImage cropImage = this.diyDataList.get(i);
            if (cropImage.getIndex() > 0) {
                arrayList.add(cropImage);
            }
        }
        arrayList.sort(new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.7
            @Override // java.util.Comparator
            public int compare(CropImage cropImage2, CropImage cropImage3) {
                if (cropImage2.getIndex() > cropImage3.getIndex()) {
                    return 1;
                }
                return Objects.equals(cropImage2.getId(), cropImage3.getId()) ? 0 : -1;
            }
        });
        return arrayList;
    }

    private List<DefaultImage> getSelectDefaultImage() {
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < this.defaultImageList.size(); i++) {
            DefaultImage defaultImage = this.defaultImageList.get(i);
            if (defaultImage.getIndex() > 0) {
                arrayList.add(defaultImage);
            }
        }
        arrayList.sort(new Comparator<DefaultImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.GallerySelectionActivity.8
            @Override // java.util.Comparator
            public int compare(DefaultImage defaultImage2, DefaultImage defaultImage3) {
                if (defaultImage2.getIndex() > defaultImage3.getIndex()) {
                    return 1;
                }
                return defaultImage2.getId() == defaultImage3.getId() ? 0 : -1;
            }
        });
        return arrayList;
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        int id = view.getId();
        if (id == R.id.iv_back) {
            finish();
            return;
        }
        int i = 0;
        if (id == R.id.iv_forward) {
            if (this.isAll) {
                this.isAll = false;
                getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
                clearSort();
                if (this.flag == 1) {
                    this.diyImageAllListAdapter.notifyDataSetChanged();
                } else {
                    this.defaultImageAllListAdapter.notifyDataSetChanged();
                }
            } else {
                this.isAll = true;
                getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_selected);
                this.curIndex = 0;
                selectAllSort();
                if (this.flag == 1) {
                    this.diyImageAllListAdapter.notifyDataSetChanged();
                } else {
                    this.defaultImageAllListAdapter.notifyDataSetChanged();
                }
            }
            if (this.flag == 1) {
                initBottomStaus();
            } else {
                initBottomStaus2();
            }
            setTvTitle();
            return;
        }
        if (id != R.id.rl_bottom_menu || App.getAppData().getDeviceList().isEmpty()) {
            return;
        }
        if (this.flag == 1) {
            List<CropImage> selectDiyData = getSelectDiyData();
            StringBuilder sb = new StringBuilder();
            if (selectDiyData.isEmpty()) {
                return;
            }
            while (i < selectDiyData.size()) {
                if (i < selectDiyData.size() - 1) {
                    sb.append(selectDiyData.get(i).getImageIndex() + ",");
                } else {
                    sb.append(selectDiyData.get(i).getImageIndex() + "");
                }
                i++;
            }
            LogUtil.d("sb:" + sb.toString());
            SPUtils.put(this.mContext, C.SP.SETTINGS_DIY_IMAGE_DATA, sb.toString());
            sendSetData(sb.toString());
            return;
        }
        List<DefaultImage> selectDefaultImage = getSelectDefaultImage();
        StringBuilder sb2 = new StringBuilder();
        if (selectDefaultImage.isEmpty()) {
            return;
        }
        while (i < selectDefaultImage.size()) {
            if (i < selectDefaultImage.size() - 1) {
                sb2.append(selectDefaultImage.get(i).getId() + ",");
            } else {
                sb2.append(selectDefaultImage.get(i).getId() + "");
            }
            i++;
        }
        LogUtil.d("sb:" + sb2.toString());
        SPUtils.put(this.mContext, C.SP.SETTINGS_IMAGE_DATA, sb2.toString());
        sendSetData(sb2.toString());
    }

    private int getSelectCount() {
        int i = 0;
        if (this.flag == 1) {
            int i2 = 0;
            while (i < this.diyDataList.size()) {
                if (this.diyDataList.get(i).getIndex() > 0) {
                    i2++;
                }
                i++;
            }
            return i2;
        }
        int i3 = 0;
        while (i < this.defaultImageList.size()) {
            if (this.defaultImageList.get(i).getIndex() > 0) {
                i3++;
            }
            i++;
        }
        return i3;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setTvTitle() {
        int selectCount = getSelectCount();
        if (selectCount > 0) {
            getBinding().rlBottomMenu.setAlpha(1.0f);
        } else {
            getBinding().rlBottomMenu.setAlpha(0.5f);
        }
        getBinding().tvTitle.setText(getString(R.string.selected) + " " + selectCount);
    }

    private void clearSort() {
        this.curIndex = 0;
        if (this.flag == 1) {
            for (int i = 0; i < this.diyDataList.size(); i++) {
                this.diyDataList.get(i).setIndex(0);
            }
            return;
        }
        for (int i2 = 0; i2 < this.defaultImageList.size(); i2++) {
            this.defaultImageList.get(i2).setIndex(0);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        clearSort();
        DiyAgreement.getInstance().clear();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void initBottomStaus() {
        if (!this.diyDataList.isEmpty()) {
            getBinding().ivForward.setAlpha(1.0f);
            getBinding().ivForward.setEnabled(true);
            getBinding().rlBottomMenu.setAlpha(1.0f);
            getBinding().rlBottomMenu.setEnabled(true);
            return;
        }
        getBinding().ivForward.setAlpha(0.5f);
        getBinding().ivForward.setEnabled(false);
        getBinding().rlBottomMenu.setAlpha(0.5f);
        getBinding().rlBottomMenu.setEnabled(false);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void initBottomStaus2() {
        if (!this.defaultImageList.isEmpty()) {
            getBinding().ivForward.setAlpha(1.0f);
            getBinding().ivForward.setEnabled(true);
            getBinding().rlBottomMenu.setAlpha(1.0f);
            getBinding().rlBottomMenu.setEnabled(true);
            return;
        }
        getBinding().ivForward.setAlpha(0.5f);
        getBinding().ivForward.setEnabled(false);
        getBinding().rlBottomMenu.setAlpha(0.5f);
        getBinding().rlBottomMenu.setEnabled(false);
    }

    private void sendSetData(String str) {
        if (ConnectActivity.getBleManager() == null) {
            return;
        }
        this.curSendIndex = 0;
        int iIntValue = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_LOOP_MODE, 0)).intValue();
        int iIntValue2 = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_GESTURE_ENABLE, 0)).intValue();
        this.imageSelectMode = ((Integer) SPUtils.get(this.mContext, C.SP.SETTINGS_IMAGE_SELECT_MODE, 0)).intValue();
        LogUtil.d("获取到的变脸数据：" + str);
        if (str != null) {
            showProgressDialog(this.mActivity, getString(R.string.send));
            List<List<Integer>> sendDataByeStr = getSendDataByeStr(str);
            this.sendDataList = sendDataByeStr;
            if (sendDataByeStr.isEmpty()) {
                return;
            }
            sendImageDataToDevice(this.dataByte.length, ByteUtils.listConvertByteArray(this.sendDataList.get(0)), iIntValue2, iIntValue, this.imageSelectMode);
        }
    }

    private void sendImageDataToDevice(int i, byte[] bArr, int i2, int i3, int i4) {
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
                this.dataByte[i2] = Byte.valueOf(strArrSplit[i2]).byteValue();
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
}
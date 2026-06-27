package cn.com.heaton.shiningmask.ui.activity;

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
import cn.com.heaton.shiningmask.databinding.ActivityImageSelectBinding;
import cn.com.heaton.shiningmask.model.data.DiyAgreement;
import cn.com.heaton.shiningmask.ui.adapter.DiyImageAllListAdapter;
import cn.com.heaton.shiningmask.ui.dialog.SyncProgressDialog;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.cdbwsoft.library.ble.BleDevice;
import com.loopj.android.http.AsyncHttpClient;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.Objects;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class ImageSelectActivity extends BaseActivity<ActivityImageSelectBinding> implements View.OnClickListener {
    private int curIndex;
    private DaoSession daoSession;
    DiyImageAllListAdapter diyImageAllListAdapter;
    private boolean isAll;
    List<CropImage> selectCropImageList;
    private SyncProgressDialog syncProgressDialog;
    private List<CropImage> diyDataList = new ArrayList();
    Handler mhandler = new Handler();
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.6
        @Override // java.lang.Runnable
        public void run() {
            ImageSelectActivity.this.hideSyncImageDialog();
        }
    };
    int sendImageIndex = 0;
    DiyAgreement.DiyAgreementListener diyAgreementListener = new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.8
        @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
        public void onFinishSend(BleDevice bleDevice) {
            LogUtil.d("=====diy数据发送完成:" + ImageSelectActivity.this.sendImageIndex);
            ImageSelectActivity.this.setSyncProgress(r0.sendImageIndex + 1, ImageSelectActivity.this.selectCropImageList.size());
            ImageSelectActivity.this.sendImageIndex++;
            if (ImageSelectActivity.this.selectCropImageList != null) {
                if (ImageSelectActivity.this.sendImageIndex < ImageSelectActivity.this.selectCropImageList.size()) {
                    CropImage cropImage = ImageSelectActivity.this.selectCropImageList.get(ImageSelectActivity.this.sendImageIndex);
                    ImageSelectActivity.this.synTimerOut();
                    ImageSelectActivity.this.sendImageData(bleDevice, cropImage);
                    return;
                } else {
                    ImageSelectActivity.this.sendImageIndex = 0;
                    ImageSelectActivity.this.dismissSyncProgressDialog();
                    return;
                }
            }
            ImageSelectActivity.this.sendImageIndex = 0;
            ImageSelectActivity.this.dismissSyncProgressDialog();
        }
    };

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityImageSelectBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityImageSelectBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivBack.setOnClickListener(this);
        getBinding().ivForward.setOnClickListener(this);
        getBinding().ivDiyPlay.setOnClickListener(this);
        getBinding().ivDiySyn.setOnClickListener(this);
        getBinding().ivDiyDelete.setOnClickListener(this);
        getBinding().ivBack.setImageResource(R.mipmap.all_diy_close);
        getBinding().ivForward.setVisibility(0);
        getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        DaoSession daoSession = App.getDaoSession();
        this.daoSession = daoSession;
        this.diyDataList = daoSession.getCropImageDao().queryBuilder().list();
        clearSort();
        this.diyImageAllListAdapter = new DiyImageAllListAdapter(this.mContext, this.diyDataList);
        getBinding().lvImage.setAdapter((ListAdapter) this.diyImageAllListAdapter);
        initBottomStaus();
        setTvTitle();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().lvImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.1
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                CropImage cropImage = (CropImage) ImageSelectActivity.this.diyDataList.get(i);
                if (cropImage.getIndex() > 0) {
                    ImageSelectActivity.this.isAll = false;
                    ((ActivityImageSelectBinding) ImageSelectActivity.this.getBinding()).ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
                    cropImage.setIndex(0);
                    for (int i2 = 0; i2 < ImageSelectActivity.this.diyDataList.size(); i2++) {
                        LogUtil.d("diyData:" + ((CropImage) ImageSelectActivity.this.diyDataList.get(i2)).toString());
                    }
                    ImageSelectActivity imageSelectActivity = ImageSelectActivity.this;
                    imageSelectActivity.diyDataSort(imageSelectActivity.diyDataList);
                } else {
                    ImageSelectActivity imageSelectActivity2 = ImageSelectActivity.this;
                    int i3 = imageSelectActivity2.curIndex + 1;
                    imageSelectActivity2.curIndex = i3;
                    cropImage.setIndex(i3);
                }
                ImageSelectActivity.this.initBottomStaus();
                ImageSelectActivity.this.setTvTitle();
                ImageSelectActivity.this.diyImageAllListAdapter.notifyDataSetChanged();
            }
        });
    }

    private void selectAllSort() {
        for (int i = 1; i <= this.diyDataList.size(); i++) {
            this.diyDataList.get(i - 1).setIndex(i);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void diyDataSort(List<CropImage> list) {
        list.sort(new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.2
            @Override // java.util.Comparator
            public int compare(CropImage cropImage, CropImage cropImage2) {
                return Integer.compare(cropImage.getIndex(), cropImage2.getIndex());
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
        list.sort(new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.3
            @Override // java.util.Comparator
            public int compare(CropImage cropImage, CropImage cropImage2) {
                return cropImage.getId().compareTo(cropImage2.getId());
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
        arrayList.sort(new Comparator<CropImage>() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.4
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

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == R.id.iv_back) {
            finish();
            return;
        }
        if (id == R.id.iv_forward) {
            if (this.isAll) {
                this.isAll = false;
                getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
                clearSort();
                this.diyImageAllListAdapter.notifyDataSetChanged();
            } else {
                this.isAll = true;
                getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_selected);
                this.curIndex = 0;
                selectAllSort();
                this.diyImageAllListAdapter.notifyDataSetChanged();
            }
            initBottomStaus();
            setTvTitle();
            return;
        }
        if (id == R.id.iv_diy_play) {
            LogUtil.d("播放选中的图片");
            List<CropImage> selectDiyData = getSelectDiyData();
            if (selectDiyData.isEmpty()) {
                return;
            }
            playSelectImage(selectDiyData);
            return;
        }
        if (id == R.id.iv_diy_syn) {
            LogUtil.d("同步选中的图片");
            synSelectImage();
        } else if (id == R.id.iv_diy_delete) {
            LogUtil.d("删除选中的图片");
            List<CropImage> selectDiyData2 = getSelectDiyData();
            if (selectDiyData2.isEmpty()) {
                return;
            }
            deleteDeviceImage(selectDiyData2);
        }
    }

    private void allSelect(boolean z) {
        if (z) {
            for (int i = 0; i < this.diyDataList.size(); i++) {
                this.diyDataList.get(i).setSelectStatus(true);
            }
        } else {
            for (int i2 = 0; i2 < this.diyDataList.size(); i2++) {
                this.diyDataList.get(i2).setSelectStatus(false);
            }
        }
        this.diyImageAllListAdapter.notifyDataSetChanged();
    }

    private int getSelectCount() {
        int i = 0;
        for (int i2 = 0; i2 < this.diyDataList.size(); i2++) {
            if (this.diyDataList.get(i2).getIndex() > 0) {
                i++;
            }
        }
        return i;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setTvTitle() {
        if (getSelectCount() > 0) {
            getBinding().rlBottomMenu.setAlpha(1.0f);
        } else {
            getBinding().rlBottomMenu.setAlpha(0.5f);
        }
        getBinding().tvTitle.setText(getResources().getString(R.string.selected) + " " + getSelectCount());
    }

    private void clearSort() {
        this.curIndex = 0;
        for (int i = 0; i < this.diyDataList.size(); i++) {
            this.diyDataList.get(i).setIndex(0);
            LogUtil.d("排序后的结果4：" + this.diyDataList.get(i).toString());
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        clearSort();
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
        getBinding().ivForward.setAlpha(0.7f);
        getBinding().ivForward.setEnabled(false);
        getBinding().rlBottomMenu.setAlpha(0.7f);
        getBinding().rlBottomMenu.setEnabled(false);
    }

    private void playSelectImage(final List<CropImage> list) {
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList.isEmpty()) {
            return;
        }
        showProgressDialog(this.mContext, this.mActivity.getString(R.string.send));
        for (int i = 0; i < deviceList.size(); i++) {
            DiyAgreement.getInstance().playDiyImage(deviceList.get(i), list, new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.5
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onPlayDiyImageOk(BleDevice bleDevice) {
                    LogUtil.d("播放成功");
                    ImageSelectActivity.this.dismissProgressDialog();
                    if (list.size() > 10) {
                        DiyAgreement.getInstance();
                        DiyAgreement.getInstance().playDiyImage(bleDevice, list, null, 1);
                    }
                }
            }, 0);
        }
    }

    private void synSelectImage() {
        List<CropImage> list;
        this.selectCropImageList = getSelectDiyData();
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList == null || deviceList.isEmpty() || (list = this.selectCropImageList) == null || list.isEmpty()) {
            return;
        }
        showSyncProgressDialog();
        synTimerOut();
        for (int i = 0; i < deviceList.size(); i++) {
            sendImageData(deviceList.get(i), this.selectCropImageList.get(this.sendImageIndex));
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void synTimerOut() {
        int size = this.diyDataList.size() * AsyncHttpClient.DEFAULT_SOCKET_TIMEOUT;
        this.mhandler.removeCallbacks(this.runnable);
        this.mhandler.postDelayed(this.runnable, size);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideSyncImageDialog() {
        SyncProgressDialog syncProgressDialog = this.syncProgressDialog;
        if (syncProgressDialog != null) {
            syncProgressDialog.dismiss();
            this.syncProgressDialog = null;
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void deleteSelectImage() {
        List<CropImage> selectDiyData = getSelectDiyData();
        for (int i = 0; i < selectDiyData.size(); i++) {
            CropImage cropImage = selectDiyData.get(i);
            this.daoSession.getCropImageDao().deleteByKey(cropImage.getId());
            for (int i2 = 0; i2 < this.diyDataList.size(); i2++) {
                if (Objects.equals(cropImage.getId(), this.diyDataList.get(i2).getId())) {
                    this.diyDataList.remove(i2);
                }
            }
        }
        this.diyImageAllListAdapter.notifyDataSetChanged();
        this.curIndex = 0;
        initBottomStaus();
        setTvTitle();
        getBinding().ivForward.setImageResource(R.mipmap.all_diy_all_unselected);
        EventBus.getDefault().post(C.MAIN_EVENT.UPDATE_DIY_LIST);
    }

    private void deleteDeviceImage(final List<CropImage> list) {
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList.isEmpty()) {
            deleteSelectImage();
            return;
        }
        showProgressDialog(this.mContext, this.mActivity.getString(R.string.send));
        for (int i = 0; i < deviceList.size(); i++) {
            DiyAgreement.getInstance().deleteDiyImage(deviceList.get(i), list, new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity.7
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onDeleteDiyImageOk(BleDevice bleDevice) {
                    LogUtil.d("删除成功");
                    ImageSelectActivity.this.dismissProgressDialog();
                    if (list.size() > 10) {
                        DiyAgreement.getInstance();
                        DiyAgreement.getInstance().deleteDiyImage(bleDevice, list, null, 1);
                    }
                    ImageSelectActivity.this.deleteSelectImage();
                }
            }, 0);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendImageData(BleDevice bleDevice, CropImage cropImage) {
        EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        DiyAgreement.getInstance().sendDiy(bleDevice, cropImage, this.diyAgreementListener);
    }

    private void showSyncProgressDialog() {
        if (this.syncProgressDialog == null) {
            this.syncProgressDialog = new SyncProgressDialog(this.mContext, R.style.dialog_clearimage);
        }
        if (this.syncProgressDialog.isShowing()) {
            return;
        }
        this.syncProgressDialog.show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setSyncProgress(float f, float f2) {
        if (f2 > 0.0f) {
            float f3 = (f / f2) * 100.0f;
            SyncProgressDialog syncProgressDialog = this.syncProgressDialog;
            if (syncProgressDialog != null) {
                syncProgressDialog.setProgress((int) f3);
            }
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void dismissSyncProgressDialog() {
        SyncProgressDialog syncProgressDialog = this.syncProgressDialog;
        if (syncProgressDialog == null || !syncProgressDialog.isShowing()) {
            return;
        }
        this.syncProgressDialog.setProgress(0);
        this.syncProgressDialog.dismiss();
    }
}
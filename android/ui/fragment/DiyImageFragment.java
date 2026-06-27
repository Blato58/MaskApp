package cn.com.heaton.shiningmask.ui.fragment;

import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.drawable.ColorDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import android.widget.PopupWindow;
import android.widget.Toast;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.PickVisualMediaRequest;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.core.content.ContextCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseFragment;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.dao.CropImageDao;
import cn.com.heaton.shiningmask.dao.DaoSession;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import cn.com.heaton.shiningmask.databinding.FragmentDiyBinding;
import cn.com.heaton.shiningmask.model.data.DiyAgreement;
import cn.com.heaton.shiningmask.ui.activity.CameraActivity;
import cn.com.heaton.shiningmask.ui.activity.ImageSelectActivity;
import cn.com.heaton.shiningmask.ui.activity.UCropActivity;
import cn.com.heaton.shiningmask.ui.adapter.DiyImageListAdapter;
import cn.com.heaton.shiningmask.ui.dialog.ClearImageDialog;
import cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog;
import cn.com.heaton.shiningmask.ui.dialog.SyncImageDialog;
import cn.com.heaton.shiningmask.ui.dialog.SyncProgressDialog;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.PopupWindowUtil;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import com.cdbwsoft.library.ble.BleDevice;
import com.loopj.android.http.AsyncHttpClient;
import com.yalantis.ucrop.UCrop;
import java.io.File;
import java.util.ArrayList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;
import org.greenrobot.eventbus.Subscribe;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class DiyImageFragment extends BaseFragment<FragmentDiyBinding> implements EasyPermissions.PermissionCallbacks, View.OnClickListener {
    private static final int GALLERY_REQUEST_CODE = 110;
    public static DiyImageFragment fragment;
    private ClearImageDialog clearImageDialog;
    private DaoSession daoSession;
    private CropImageDao diyDataDao;
    private DiyImageListAdapter diyImageListAdapter;
    private Uri mDestination;
    private PopupWindow mPopupWindow;
    ActivityResultLauncher<PickVisualMediaRequest> pickMedia;
    private PicturesChooseDialog picturesChooseDialog;
    private SyncImageDialog syncImageDialog;
    private SyncProgressDialog syncProgressDialog;
    private List<CropImage> diyDataList = new ArrayList();
    Runnable setImageNumRunnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.9
        @Override // java.lang.Runnable
        public void run() {
            DiyImageFragment.this.getDeviceDiyImageNum();
        }
    };
    boolean isSyn1 = false;
    int sendImageIndex = 0;
    DiyAgreement.DiyAgreementListener diyAgreementListener = new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.13
        @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
        public void onFinishSend(BleDevice bleDevice) {
            LogUtil.d("=====diy数据发送完成:" + DiyImageFragment.this.sendImageIndex);
            DiyImageFragment.this.setSyncProgress(r3.sendImageIndex + 1, DiyImageFragment.this.diyDataList.size());
            DiyImageFragment.this.sendImageIndex++;
            if (DiyImageFragment.this.diyDataList != null) {
                if (DiyImageFragment.this.sendImageIndex < DiyImageFragment.this.diyDataList.size()) {
                    CropImage cropImage = (CropImage) DiyImageFragment.this.diyDataList.get(DiyImageFragment.this.sendImageIndex);
                    DiyImageFragment.this.synTimerOut();
                    DiyImageFragment.this.sendImageData(cropImage);
                    return;
                } else {
                    DiyImageFragment.this.sendImageIndex = 0;
                    DiyImageFragment.this.dismissSyncProgressDialog();
                    return;
                }
            }
            DiyImageFragment.this.sendImageIndex = 0;
            DiyImageFragment.this.dismissSyncProgressDialog();
        }
    };
    Handler mhandler = new Handler();
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.14
        @Override // java.lang.Runnable
        public void run() {
            DiyImageFragment.this.hideSyncImageDialog();
        }
    };

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsDenied(int i, List<String> list) {
    }

    public static DiyImageFragment newInstance() {
        LogUtil.d("DiyImageFragment ");
        if (fragment == null) {
            fragment = new DiyImageFragment();
        }
        return fragment;
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    public FragmentDiyBinding inflateBinding(LayoutInflater layoutInflater, ViewGroup viewGroup) {
        return FragmentDiyBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initView(View view, Bundle bundle) {
        getBinding().top.ivBack.setOnClickListener(this);
        getBinding().top.tvTitle.setOnClickListener(this);
        getBinding().top.ivForward.setOnClickListener(this);
        getBinding().ivDiyClear.setOnClickListener(this);
        getBinding().ivDiySelect.setOnClickListener(this);
        getBinding().top.tvTitle.setText(getResources().getString(R.string.gallery));
        getBinding().top.ivForward.setImageResource(R.mipmap.diy_top_add);
        getBinding().top.ivForward.setVisibility(0);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setImageNum(int i) {
        getBinding().tvDeviceCapacity.setText(i + "/20");
        float f = i / 20.0f;
        int i2 = (int) (100.0f * f);
        LogUtil.d("pro:" + f + "pro2:" + i2);
        getBinding().pbCapacity.setProgress(i2);
    }

    @Override // androidx.fragment.app.Fragment
    public void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        this.pickMedia = registerForActivityResult(new ActivityResultContracts.PickVisualMedia(), new ActivityResultCallback() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment$$ExternalSyntheticLambda0
            @Override // androidx.activity.result.ActivityResultCallback
            public final void onActivityResult(Object obj) {
                this.f$0.lambda$onCreate$0((Uri) obj);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public /* synthetic */ void lambda$onCreate$0(Uri uri) {
        closePicturesDialog();
        if (uri != null) {
            startCropActivity(uri);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initData() {
        if (!EventBus.getDefault().isRegistered(this)) {
            EventBus.getDefault().register(this);
        }
        LogUtil.d("initData ");
        DaoSession daoSession = App.getDaoSession();
        this.daoSession = daoSession;
        CropImageDao cropImageDao = daoSession.getCropImageDao();
        this.diyDataDao = cropImageDao;
        this.diyDataList = cropImageDao.queryBuilder().list();
        this.diyImageListAdapter = new DiyImageListAdapter(getActivity(), this.diyDataList);
        getBinding().lvImage.setAdapter((ListAdapter) this.diyImageListAdapter);
        getDeviceDiyImageNum();
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.1
            @Override // java.lang.Runnable
            public void run() {
                if (DataManager.getInstance().isSnyImage()) {
                    return;
                }
                DiyImageFragment.this.synImage();
            }
        }, 200L);
        getBinding().lvImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.2
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                CropImage cropImage = (CropImage) DiyImageFragment.this.diyDataList.get(i);
                LogUtil.d("选中的diy图片：" + cropImage.getId() + "  =" + cropImage.getImageIndex());
                DiyImageFragment.this.playSelectImage(cropImage);
                DiyImageFragment.this.diyImageListAdapter.setSelectPosition(i);
                DiyImageFragment.this.diyImageListAdapter.notifyDataSetChanged();
            }
        });
    }

    private void showPopupWindow(View view, int i) {
        View popupWindowContentView = getPopupWindowContentView(i);
        PopupWindow popupWindow = new PopupWindow(popupWindowContentView, -2, -2, true);
        this.mPopupWindow = popupWindow;
        popupWindow.setBackgroundDrawable(new ColorDrawable());
        int[] iArrCalculatePopWindowPos = PopupWindowUtil.calculatePopWindowPos(view, popupWindowContentView);
        this.mPopupWindow.showAtLocation(view, 8388659, iArrCalculatePopWindowPos[0] / 2, iArrCalculatePopWindowPos[1] - ScreenUtils.dp2px(this.mContext, 8.5f));
    }

    private View getPopupWindowContentView(final int i) {
        View viewInflate = LayoutInflater.from(getActivity()).inflate(R.layout.popup_content_layout, (ViewGroup) null);
        viewInflate.findViewById(R.id.iv_edit).setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.3
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
            }
        });
        viewInflate.findViewById(R.id.iv_delete).setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.4
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                if (DiyImageFragment.this.mPopupWindow != null) {
                    DiyImageFragment.this.mPopupWindow.dismiss();
                    DiyImageFragment.this.daoSession.getCropImageDao().delete((CropImage) DiyImageFragment.this.diyDataList.get(i));
                    DiyImageFragment.this.diyDataList.remove(i);
                    DiyImageFragment.this.diyImageListAdapter.setList(DiyImageFragment.this.diyDataList);
                    DiyImageFragment.this.diyImageListAdapter.notifyDataSetChanged();
                    Toast.makeText(DiyImageFragment.this.getActivity(), DiyImageFragment.this.getResources().getString(R.string.delete_succe), 0).show();
                }
            }
        });
        return viewInflate;
    }

    @Override // androidx.fragment.app.Fragment
    public void onActivityResult(int i, int i2, Intent intent) {
        super.onActivityResult(i, i2, intent);
        LogUtil.d("" + i2);
        if (i == 110) {
            closePicturesDialog();
            if (intent != null) {
                startCropActivity(intent.getData());
            }
        }
    }

    public void updateAdapter() {
        this.diyDataList = this.diyDataDao.queryBuilder().list();
        this.diyImageListAdapter = new DiyImageListAdapter(getActivity(), this.diyDataList);
        getBinding().lvImage.setAdapter((ListAdapter) this.diyImageListAdapter);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment, androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
        if (DiyAgreement.getInstance() != null) {
            DiyAgreement.getInstance().clear();
        }
        closePicturesDialog();
        ClearImageDialog clearImageDialog = this.clearImageDialog;
        if (clearImageDialog != null) {
            clearImageDialog.dismiss();
            this.clearImageDialog = null;
        }
        EventBus.getDefault().unregister(this);
    }

    @Subscribe
    public void updateData(String str) {
        if (str.equals(C.MAIN_EVENT.UPDATE_DIY_LIST)) {
            LogUtil.d("刷新diy图片列表");
            updateAdapter();
            this.mhandler.removeCallbacks(this.setImageNumRunnable);
            this.mhandler.postDelayed(this.setImageNumRunnable, 500L);
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        int id = view.getId();
        if (id == R.id.iv_back) {
            this.mActivity.finish();
            return;
        }
        if (id == R.id.iv_forward) {
            List<CropImage> listLoadAll = this.diyDataDao.loadAll();
            if (listLoadAll != null && listLoadAll.size() >= 20) {
                ToastUtil.showToast(getResources().getString(R.string.tip_image_count));
                return;
            } else {
                showPicturesChooseDialog();
                return;
            }
        }
        if (id == R.id.iv_diy_clear) {
            showClearDialog();
        } else if (id == R.id.iv_diy_select) {
            startActivity(new Intent(this.mContext, (Class<?>) ImageSelectActivity.class));
        }
    }

    private void showClearDialog() {
        if (this.clearImageDialog == null) {
            ClearImageDialog clearImageDialog = new ClearImageDialog(this.mContext, R.style.dialog_clearimage);
            this.clearImageDialog = clearImageDialog;
            clearImageDialog.setResultListener(new ClearImageDialog.ResultListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.5
                @Override // cn.com.heaton.shiningmask.ui.dialog.ClearImageDialog.ResultListener
                public void phone() {
                    DiyImageFragment.this.clearImageDialog.dismiss();
                    DiyImageFragment.this.clearPhoneAllImage();
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.ClearImageDialog.ResultListener
                public void device() {
                    DiyImageFragment.this.clearImageDialog.dismiss();
                    DiyImageFragment.this.clearDeviceAllImage(0);
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.ClearImageDialog.ResultListener
                public void phoneAndDevice() {
                    DiyImageFragment.this.clearImageDialog.dismiss();
                    DiyImageFragment.this.clearDeviceAllImage(1);
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.ClearImageDialog.ResultListener
                public void close() {
                    DiyImageFragment.this.clearImageDialog.dismiss();
                }
            });
        }
        if (this.clearImageDialog.isShowing()) {
            return;
        }
        this.clearImageDialog.show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showSyncImageDialog() {
        if (this.syncImageDialog == null) {
            SyncImageDialog syncImageDialog = new SyncImageDialog(this.mContext, R.style.dialog_clearimage);
            this.syncImageDialog = syncImageDialog;
            syncImageDialog.setResultListener(new SyncImageDialog.ResultListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.6
                @Override // cn.com.heaton.shiningmask.ui.dialog.SyncImageDialog.ResultListener
                public void sync() {
                    LogUtil.d("同步");
                    DiyImageFragment.this.syncImageDialog.dismiss();
                    if (DiyImageFragment.this.diyDataList == null || DiyImageFragment.this.diyDataList.isEmpty()) {
                        return;
                    }
                    DiyImageFragment.this.showSyncProgressDialog();
                    DiyImageFragment.this.synTimerOut();
                    DiyImageFragment diyImageFragment = DiyImageFragment.this;
                    diyImageFragment.sendImageData((CropImage) diyImageFragment.diyDataList.get(DiyImageFragment.this.sendImageIndex));
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.SyncImageDialog.ResultListener
                public void close() {
                    DiyImageFragment.this.syncImageDialog.dismiss();
                }
            });
        }
        if (this.syncImageDialog.isShowing()) {
            return;
        }
        this.syncImageDialog.show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void hideSyncImageDialog() {
        SyncProgressDialog syncProgressDialog = this.syncProgressDialog;
        if (syncProgressDialog != null) {
            syncProgressDialog.dismiss();
            this.syncProgressDialog = null;
        }
    }

    private void showPicturesChooseDialog() {
        if (this.picturesChooseDialog == null) {
            PicturesChooseDialog picturesChooseDialog = new PicturesChooseDialog(this.mActivity, R.style.dialog_clearimage);
            this.picturesChooseDialog = picturesChooseDialog;
            picturesChooseDialog.setResultListener(new PicturesChooseDialog.ResultListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.7
                @Override // cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog.ResultListener
                public void importImage() {
                    if (ClickFilter.filter()) {
                        return;
                    }
                    DiyImageFragment.this.pickMedia.launch(new PickVisualMediaRequest.Builder().setMediaType(ActivityResultContracts.PickVisualMedia.ImageOnly.INSTANCE).build());
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog.ResultListener
                public void camera() {
                    LogUtil.d("拍照");
                    if (ClickFilter.filter()) {
                        return;
                    }
                    DiyImageFragment.this.gotoCamare();
                }
            });
        }
        if (this.picturesChooseDialog.isShowing()) {
            return;
        }
        this.picturesChooseDialog.show();
    }

    public void gotoCamare() {
        CameraActivity.startMe(this.mActivity, 1);
    }

    private void closePicturesDialog() {
        PicturesChooseDialog picturesChooseDialog = this.picturesChooseDialog;
        if (picturesChooseDialog != null) {
            picturesChooseDialog.dismiss();
            this.picturesChooseDialog = null;
        }
    }

    public void startCropActivity(Uri uri) {
        this.mDestination = Uri.fromFile(new File(this.mActivity.getCacheDir(), "cropImage" + System.currentTimeMillis() + ".png"));
        UCrop.Options options = new UCrop.Options();
        options.setToolbarColor(ContextCompat.getColor(this.mActivity, pub.devrel.easypermissions.R.color.colorPrimary));
        options.setStatusBarColor(ContextCompat.getColor(this.mActivity, pub.devrel.easypermissions.R.color.colorPrimaryDark));
        options.setCropFrameColor(0);
        options.setShowCropGrid(false);
        options.setHideBottomControls(true);
        options.setCompressionFormat(Bitmap.CompressFormat.PNG);
        options.setCompressionQuality(100);
        UCrop.of(uri, this.mDestination).withAspectRatio(46.0f, 58.0f).withMaxResultSize(46, 58).withOptions(options).start(this.mActivity, UCropActivity.class, 1);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void clearPhoneAllImage() {
        this.diyDataList.clear();
        this.diyDataDao.deleteAll();
        this.diyImageListAdapter.notifyDataSetChanged();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void clearDeviceAllImage(final int i) {
        new ArrayList();
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList.isEmpty()) {
            return;
        }
        showProgressDialog(getActivity(), getString(R.string.send));
        final ArrayList arrayList = new ArrayList();
        int i2 = 0;
        while (i2 < 20) {
            CropImage cropImage = new CropImage();
            i2++;
            cropImage.setImageIndex(i2);
            arrayList.add(cropImage);
        }
        for (int i3 = 0; i3 < deviceList.size(); i3++) {
            DiyAgreement.getInstance().deleteDiyImage(deviceList.get(i3), arrayList, new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.8
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onDeleteDiyImageOk(BleDevice bleDevice) {
                    LogUtil.d("删除成功");
                    DiyImageFragment.this.dismissProgressDialog();
                    if (arrayList.size() > 10) {
                        DiyAgreement.getInstance();
                        DiyAgreement.getInstance().deleteDiyImage(bleDevice, arrayList, null, 1);
                    }
                    if (i == 1) {
                        DiyImageFragment.this.clearDeviceAndDeviceImage();
                    }
                    DiyImageFragment.this.mhandler.removeCallbacks(DiyImageFragment.this.setImageNumRunnable);
                    DiyImageFragment.this.mhandler.postDelayed(DiyImageFragment.this.setImageNumRunnable, 500L);
                }
            }, 0);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void getDeviceDiyImageNum() {
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList.isEmpty()) {
            return;
        }
        for (int i = 0; i < deviceList.size(); i++) {
            DiyAgreement.getInstance().getDeviceImageNum(deviceList.get(i), new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.10
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onGetDiyImageCountOk(int i2) {
                    LogUtil.d("获取图片数量成功:" + i2);
                    DiyImageFragment.this.setImageNum(i2);
                }
            });
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void synImage() {
        List<CropImage> list = this.diyDataList;
        if (list == null || list.isEmpty()) {
            return;
        }
        List<CropImage> list2 = this.diyDataList;
        CropImage cropImage = list2.get(list2.size() - 1);
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i = 0; i < this.diyDataList.size(); i++) {
            LogUtil.d("图片index:" + this.diyDataList.get(i).getImageIndex() + " time:" + this.diyDataList.get(i).getTimeInt());
        }
        if (deviceList.isEmpty()) {
            return;
        }
        DataManager.getInstance().setSnyImage(true);
        for (int i2 = 0; i2 < deviceList.size(); i2++) {
            DiyAgreement.getInstance().isSynImage(deviceList.get(i2), 0, cropImage.getTimeInt(), new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.11
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onIsSynDiyImage(boolean z) {
                    DiyImageFragment.this.isSyn1 = z;
                    if (DiyImageFragment.this.isSyn1) {
                        DiyImageFragment.this.showSyncImageDialog();
                    }
                }
            });
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void clearDeviceAndDeviceImage() {
        this.diyDataList.clear();
        this.diyDataDao.deleteAll();
        setImageNum(0);
        this.diyImageListAdapter.notifyDataSetChanged();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void playSelectImage(CropImage cropImage) {
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList.isEmpty()) {
            return;
        }
        ArrayList arrayList = new ArrayList();
        arrayList.add(cropImage);
        for (int i = 0; i < deviceList.size(); i++) {
            DiyAgreement.getInstance().playDiyImage(deviceList.get(i), arrayList, new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DiyImageFragment.12
                @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
                public void onPlayDiyImageOk(BleDevice bleDevice) {
                    LogUtil.d("播放成功");
                }
            }, 0);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendImageData(CropImage cropImage) {
        EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        if (deviceList == null || deviceList.isEmpty()) {
            return;
        }
        DiyAgreement diyAgreement = DiyAgreement.getInstance();
        for (int i = 0; i < deviceList.size(); i++) {
            diyAgreement.sendDiy(deviceList.get(i), cropImage, this.diyAgreementListener);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void synTimerOut() {
        int size = this.diyDataList.size() * AsyncHttpClient.DEFAULT_SOCKET_TIMEOUT;
        this.mhandler.removeCallbacks(this.runnable);
        this.mhandler.postDelayed(this.runnable, size);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showSyncProgressDialog() {
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

    @Override // androidx.fragment.app.Fragment, androidx.core.app.ActivityCompat.OnRequestPermissionsResultCallback
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        EasyPermissions.onRequestPermissionsResult(i, strArr, iArr, this);
        LogUtil.d("onRequestPermissionsResult");
    }

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsGranted(int i, List<String> list) {
        LogUtil.d("onPermissionsGranted:");
    }
}
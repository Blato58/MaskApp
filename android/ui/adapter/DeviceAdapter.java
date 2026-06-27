package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.View;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.core.content.ContextCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseAdapter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.cdbwsoft.library.ble.BleDevice;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class DeviceAdapter extends BaseAdapter<BleDevice> {
    private int mCheckedItemPosition;
    private OnConnectListener onConnectListener;

    public interface OnConnectListener {
        void connect(BleDevice bleDevice);

        void onLongClick(BleDevice bleDevice);

        void rename(BleDevice bleDevice);
    }

    public DeviceAdapter(Context context, List<BleDevice> list) {
        super(context, list);
    }

    public void setOnlistener(OnConnectListener onConnectListener) {
        this.onConnectListener = onConnectListener;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseAdapter
    public int getContentView() {
        return R.layout.item_device;
    }

    public void setCheckedItemPosition(int i) {
        this.mCheckedItemPosition = i;
    }

    @Override // cn.com.heaton.shiningmask.base.BaseAdapter
    public void onInitView(View view, int i) {
        final BleDevice item = getItem(i);
        ImageView imageView = (ImageView) getAdapterView(view, R.id.cb_connect);
        TextView textView = (TextView) getAdapterView(view, R.id.tv_name);
        RelativeLayout relativeLayout = (RelativeLayout) getAdapterView(view, R.id.ll_root);
        textView.setText(item.getBleName());
        if (item.isConnected()) {
            textView.setTextColor(ContextCompat.getColor(this.mContext, R.color.item_device_title_selected));
            imageView.setImageResource(R.mipmap.magic_device_on);
            relativeLayout.setBackgroundResource(R.mipmap.magic_device_item_bg1);
        } else {
            textView.setTextColor(ContextCompat.getColor(this.mContext, R.color.item_device_title_unselected));
            imageView.setImageResource(R.mipmap.magic_device_off);
            relativeLayout.setBackgroundResource(R.mipmap.magic_device_item_bg);
        }
        textView.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view2) {
                if (DeviceAdapter.this.onConnectListener != null) {
                    DeviceAdapter.this.onConnectListener.rename(item);
                }
            }
        });
        imageView.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.2
            @Override // android.view.View.OnClickListener
            public void onClick(View view2) {
                if (DeviceAdapter.this.onConnectListener != null) {
                    DeviceAdapter.this.onConnectListener.connect(item);
                }
            }
        });
        textView.setOnLongClickListener(new View.OnLongClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.3
            @Override // android.view.View.OnLongClickListener
            public boolean onLongClick(View view2) {
                LogUtil.d("===onLongClick111===");
                if (DeviceAdapter.this.onConnectListener == null) {
                    return false;
                }
                DeviceAdapter.this.onConnectListener.onLongClick(item);
                return false;
            }
        });
    }
}
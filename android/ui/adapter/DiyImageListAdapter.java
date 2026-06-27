package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.LinearLayout;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import com.bumptech.glide.Glide;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class DiyImageListAdapter extends BaseAdapter {
    List<CropImage> brandsList;
    Context context;
    LayoutInflater mInflater;
    private int selectPosition = -1;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public DiyImageListAdapter(Context context, List<CropImage> list) {
        this.context = context;
        this.brandsList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<CropImage> list) {
        this.brandsList = list;
    }

    public void setSelectPosition(int i) {
        this.selectPosition = i;
    }

    public int getSelectPosition() {
        return this.selectPosition;
    }

    @Override // android.widget.Adapter
    public int getCount() {
        return this.brandsList.size();
    }

    @Override // android.widget.Adapter
    public Object getItem(int i) {
        return Integer.valueOf(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        ViewHolder viewHolder;
        if (view == null) {
            view = this.mInflater.inflate(R.layout.item_image_adapter1, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.iv_crop_image = (ImageView) view.findViewById(R.id.iv_crop_image);
            viewHolder.ll_ledView = (LinearLayout) view.findViewById(R.id.ll_ledView);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        Glide.with(this.context).load(this.brandsList.get(i).getImageUrl()).fitCenter().error(R.mipmap.ic_launcher).into(viewHolder.iv_crop_image);
        if (this.selectPosition == i) {
            viewHolder.ll_ledView.setBackgroundResource(R.mipmap.item_image_deuault_bg_unselected);
            return view;
        }
        viewHolder.ll_ledView.setBackgroundResource(R.mipmap.item_image_deuault_bg_selected);
        return view;
    }

    public class ViewHolder {
        ImageView iv_crop_image;
        LinearLayout ll_ledView;

        public ViewHolder() {
        }
    }
}
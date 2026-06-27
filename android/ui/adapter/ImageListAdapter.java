package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.LinearLayout;
import cn.com.heaton.shiningmask.R;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class ImageListAdapter extends BaseAdapter {
    List<Integer> brandsList;
    Context context;
    LayoutInflater mInflater;
    private int selectPosition = -1;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public ImageListAdapter(Context context, List<Integer> list) {
        this.context = context;
        this.brandsList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<Integer> list) {
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
            view = this.mInflater.inflate(R.layout.item_image_default_adapter, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.iv_item = (ImageView) view.findViewById(R.id.item_ledview);
            viewHolder.ll_ledView1 = (LinearLayout) view.findViewById(R.id.ll_ledView1);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        viewHolder.iv_item.setImageResource(this.brandsList.get(i).intValue());
        if (this.selectPosition == i) {
            viewHolder.iv_item.setAlpha(1.0f);
            viewHolder.ll_ledView1.setBackgroundResource(R.mipmap.item_image_deuault_bg_unselected);
            return view;
        }
        viewHolder.iv_item.setAlpha(0.65f);
        viewHolder.ll_ledView1.setBackgroundResource(R.mipmap.item_image_deuault_bg_selected);
        return view;
    }

    public class ViewHolder {
        ImageView iv_item;
        LinearLayout ll_ledView1;

        public ViewHolder() {
        }
    }
}
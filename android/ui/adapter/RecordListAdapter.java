package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.dao.bean.InputTextRecord;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class RecordListAdapter extends BaseAdapter {
    Context context;
    LayoutInflater mInflater;
    List<InputTextRecord> recordList;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public RecordListAdapter(Context context, List<InputTextRecord> list) {
        this.context = context;
        this.recordList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<InputTextRecord> list) {
        this.recordList = list;
    }

    @Override // android.widget.Adapter
    public int getCount() {
        List<InputTextRecord> list = this.recordList;
        if (list == null) {
            return 0;
        }
        if (list.size() > 10) {
            return 10;
        }
        return this.recordList.size();
    }

    @Override // android.widget.Adapter
    public Object getItem(int i) {
        return Integer.valueOf(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        ViewHolder viewHolder;
        if (view == null) {
            view = this.mInflater.inflate(R.layout.item_record, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.tvContent = (TextView) view.findViewById(R.id.tv_content);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        viewHolder.tvContent.setText(this.recordList.get(i).getTextContent());
        return view;
    }

    public class ViewHolder {
        TextView tvContent;

        public ViewHolder() {
        }
    }
}
import '../css/components.css';

export default function RawHtmlTable(props: any) {
    let data = props.data;
    let cols = props.cols;

    function thead() {
        return (
            <thead>
                <tr>
                    {cols.map((c: any) => { return (<td key={c.title}>{c.title}</td>) })}
                </tr>
            </thead>
        );
    }

    function tbody() {
        return (
            <tbody>{data.map((d: any, index: any) => trow(d, index))}</tbody>
        )
    }

    function trow(d: any, index: any) {
        return (
            <tr key={index}>
                {
                    cols.map((c: any, cindex: any) => { return (<td key={index.toString() + cindex.toString()}><pre>{d[c.key]}</pre></td>); })
                }
            </tr>
        )
    }

    return (
        <div className="rawtable">
            <table>
                {thead()}
                {tbody()}
            </table>
        </div>
    )
}
import urls from "@dn/shared/ApiUrls";
import Config from "@dn/shared/Config";
import { useEffect, useState } from "react";

export default function System() {
    const [system, setSystem] = useState(null);

    useEffect(() => {
        fetch(urls.home.System).then(r => r.json()).then(r => setSystem(r));
    }, []);

    return (
        <pre>
            {JSON.stringify(Config, null, 4)}
            <br />
            {system ? JSON.stringify(system, null, 4) : ""}
        </pre>
    );
}
(function () {
    let gtagScript = document.createElement('script');
    gtagScript.src = 'https://www.googletagmanager.com/gtag/js?id=G-XP7D8QMFZK';
    document.head.append(gtagScript);

    let gtagSetup = document.createElement('script');
    gtagSetup.text =
        `
    function setupGTag() {
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}

        gtag('js', new Date());
        gtag('config', 'G-XP7D8QMFZK');
        
    }

     /* Disable analytics on development */
    if (!window.location.host.includes('localhost:')) {
        setupGTag();
    }
`;

    let footerEl = document.getElementById('id-dndocs-footer');
    let dataInfo = document.getElementById('id-dndocs-info-json');
    let versionEl = '';

    // todo maye add here dev url?
    // and do not  render gtag on dev env
    if (dataInfo && dataInfo.innerHTML) {
        try {
            dataInfo = JSON.parse(dataInfo.innerHTML);
            if (dataInfo.isVersioning) {
                versionEl = `<a href="https://dndocs.com/project-versions/${dataInfo.projectId}">Versions</a>`;
            }
        } catch { /* ?  send error to API? */}
    }

    // debugger;
    

    let footerHtml =
`
<div style="display: flex; align-items: center;">
    <div style="flex: 1;">${versionEl}</div>
    <div>
     <a href="https://dndocs.com">Hosted with DNDocs</a>
     <span style="display: inline-block; margin: 0 0.5em;">|</span>
     <a href="https://dotnet.github.io/docfx">Docfx</a>
   </div>
   <div style=\"flex: 1;\"></div>
</div>
`;

    
    footerEl.style = 'width: 100%';

    footerEl.insertAdjacentHTML("afterbegin", footerHtml);

    /* gtag*/
    document.head.append(gtagSetup);

    /* cookies consent */
    
    return; // todo implement this later
    var consentTrue = !!document.cookie.split("; ").find((row) => row.startsWith("DndocsCookiesConsent"));
    if (!consentTrue) {
        fetch('https://docs.dndocs.com/public/cookies-consent.html')
            .then(r => {
                if (r.ok) { return r.text() }
                else { throw new Error('not loaded');  }
            })
            .then(r => {
                const slotHtml = document.createRange().createContextualFragment(r)
                document.body.appendChild(slotHtml);
            });
    }
})()
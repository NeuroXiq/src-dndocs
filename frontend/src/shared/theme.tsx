import { createTheme } from '@mui/material/styles';
import { red, orange } from '@mui/material/colors';
// Create a theme instance.
const theme = createTheme({
   palette: {
      // #130058
      // primary: {
      //    main: '#43bc00',
      //    contrastText: '#fff',
      // },
      primary: {
         main: '#130058',
         contrastText: '#fff'
      },
      text: {
         primary: '#000',
      },
      // secondary: {
      //    contrastText: '#fff',
      //    main: '#009400',
      //    light: '#000'
      // },
      secondary: {
         main: '#590368',
         contrastText: '#fff'
      },
      error: {
         contrastText: '#fff',
         main: red.A400,
      },
      warning: {
         contrastText: '#fff',
         light: orange.A400,
         main: orange.A400,
         dark: orange.A400
      },
      info: {
         main: '#29b6f6',
         dark: '#0288d1',
         light: '#4fc3f7',
         contrastText: '#FFFFFF'
      }
   },
});

export default theme;


//
// :root {
//     --font-size-1: 8px;
//     --font-size-2: 14px;
//     --font-size-3: 16px;
//     --font-size-4: 18px;
//     --font-size-5: 24px;
//     --font-size-6: 30px;
//     --font-size-7: 38px;
//     --font-size-8: 48px;
//
//     --height-footer: 192px;
//
//     --c-100: #DCEDC8;
//     --c-200: #C5E1A5;
//     --c-300: #AED581;
//     --c-400: #9CCC65;
//     --c-500: #8BC34A;
//     --c-600: #7CB342;
//     --c-700: #689F38;
//     --c-800: #558B2F;
//     --c-900: #33691E;
//     --c-1000: #133900;
//
//     /*--c-100: #DCEDC8;
//     --c-200: #C5E1A5;
//     --c-300: #AED581;
//     --c-400: #9CCC65;
//     --c-500: #8BC34A;
//     --c-600: #7CB342;
//     --c-700: #689F38;
//     --c-800: #558B2F;
//     --c-900: #33691E;
//     --c-1000: #133900;*/
//
//     --c-p50: #edfae6;
//     --c-p100: #d2f1c0;
//     --c-p200: #b2e797;
//     --c-p300: #90dd6a;
//     --c-p400: #73d544;
//     --c-p500: #55cc0d;
//     /*main*/
//     --c-p600: #43bc00;
//     --c-p700: #24a800;
//     --c-p800: #009400;
//     --c-p900: #007100;
//
//     --c-c50: #f4e4f8;
//     --c-c100: #e3bcee;
//     --c-c200: #d18fe5;
//     --c-c300: #be61da;
//     --c-c400: #af3ad2;
//     --c-c500: #a000c9;
//     --c-c600: #9000c3;
//     /*main*/
//     --c-c700: #7a00bc;
//     --c-c800: #6600b6;
//     --c-c900: #3900ac;
//
//
//     --c-A100: #CCFF90;
//     --c-A200: #B2FF59;
//     --c-A400: #76FF03;
//     --c-A700: #64DD17;
//     --c-error: #B00020;
//
//     /*primary*/
//     --c-p: var(--c-p600);
//     /*primary variant*/
//     --c-pv: var(--c-p800);
//     /*secondary*/
//     --c-s: var(--c-c700);
//     /*secondary variant*/
//     --c-sv: var(--c-c900);
//
//     /* background */
//     --c-bg:white;
//     /*surface*/
//     --c-sf: white;
//     /*error*/
//     --c-er: var(--c-error);
//     /*onprimary*/
//     --c-op: white;
//     /*onsecondary*/
//     --c-os: white;
//     /*onbackground*/
//     --c-obg: black;
//     /*onsurface*/
//     --c-osf: black;
//     /*onerror*/
//     --c-oe: var(--c-200);
//
//     --c-border: var(--c-c700);
//     --border-radius: 0.25rem;
//     --border-width: 2px;
//
//
//     --font-regular: 'Lato-Regular';
//     --font-bold: 'Lato-Bold';
//
//     --height-input: 1.5rem;
//     --padding-input: 0.5em;
//     --max-width-content: calc(100vw - 600px);
//     --zindex-globaloverlay: 100;
//     --z-index-toastmsg-container: 101;
// }

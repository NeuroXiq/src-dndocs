import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Urls from 'config/Urls';
import MUILink from '@mui/material/Link';

import { Divider, Grid, Stack } from '@mui/material';
import consts from 'config/const';


export default function Footer() {

  const muLink = function (title: any, url: any) {
    return (<MUILink underline='none' color="white" variant="body2" href={url} >{title}</MUILink>)
  };

  return (
    <Box component="footer" sx={{ mt: 0, color: 'white', backgroundColor: 'primary.main' }}>
      <Divider />
      <Grid container spacing={1} sx={{ mt: 0 }}>
        <Grid item lg={1}>
        </Grid>
        <Grid lg={10} item>
          <Stack flexDirection="row" gap={2} alignItems="center" justifyContent="center">
            <Typography margin={1} variant="body2" textAlign="center">
              Powered by  <a>{consts.AppNameUI}</a> /
              &copy; 2023 - {(new Date().getFullYear())}. All Rights Reserved
            </Typography>

            {muLink('Home', Urls.home.index)}
            {muLink('How to use', Urls.home.howToUse)}
            {muLink('Terms of service', Urls.home.termsOfService)}
            {muLink('All Projects', Urls.home.projects)}
          </Stack>
        </Grid>
        <Grid item lg={1}>

        </Grid>
        <Grid item xs={2} sx={{ display: 'flex', flexDirection: 'row', alignItems: 'center' }}>
          
        </Grid>
       
      </Grid>
    </Box>
  );
}
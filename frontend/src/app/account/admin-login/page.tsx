'use client';
import { Box, FormControl, FormHelperText, Grid, Input, InputLabel, Paper, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import { useState } from 'react';
import UseApi from '../../../../services/Api';
import { apiCallNoEffect } from 'hooks/useApiCall';
import UseUser from '@/services/user';


export default function Index() {
  const [pwd, setPwd] = useState('');
  const [formLogin, setFormLogin] = useState('');
  const [loginSuccess, setLoginSuccess] = useState(false);
  const api = UseApi();
  const { user, login } = UseUser();

  const apiCall = apiCallNoEffect();


  const onSubmit = function () {
    api.adminLogin({login: formLogin, password: pwd}).then(r => {
      setLoginSuccess(r.success);
      login(r.result);
    });
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      {loginSuccess ? <Typography>Login success</Typography> : null}
      <Paper sx={{ padding: '1rem', minWidth: '40%' }}>
        <Typography variant="h5">Admin Login</Typography>
        <InputLabel htmlFor="login">Login</InputLabel>
          <Input type="text" name="login" onChange={(e) => setFormLogin(e.target.value)}></Input>
        <FormControl fullWidth>
          <InputLabel htmlFor="password">Password</InputLabel>
          <Input fullWidth id="password" name="password" type="password" onChange={e => setPwd(e.target.value)} />
          <Button type="submit" onClick={onSubmit}>Submit</Button>
        </FormControl>
      </Paper>
    </Box >
  )
}
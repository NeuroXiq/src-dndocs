'use client';
import { Box, Grid, TablePagination, alpha, styled } from '@mui/material';
import Button from '@mui/material/Button';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { Search } from '@mui/icons-material';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import InputBase from '@mui/material/InputBase';
import MenuIcon from '@mui/icons-material/Menu';
import SearchIcon from '@mui/icons-material/Search';
import Link from 'next/link';
import { useState } from 'react';
import UseApi, { useQueryEffect } from '@/services/Api';
import Layout from '@/components/Layout';
import PageLoading from '@/components/PageLoading';
import ProjectCard from '@/components/projectCard';
import SimpleTable from '@/components/SimpleTable';
import Typoheader from '@/components/Typoheader';
import ProjectDto from '@/services/ApiDTO/Project/ProjectDto';
import Urls from 'config/Urls';
import { TableCfg } from '@/components/SimpleTable';

export default function Index() {
  const api = UseApi();

  const { loading, result } = useQueryEffect(() => api.Home_GetAllProjects());

  if (loading) {
    return (<PageLoading open={true}></PageLoading>)
  }

  let tconfig: TableCfg<ProjectDto> = {
    cols: [
      { id: 'id', title: 'Id', val: p => p.id },
      { id: 'name', title: 'Project Name', val: p => p.projectName },
      { id: 'gturl', title: 'Github URL', val: p => { return (<a href={p.githubUrl} target="_blank">{p.githubUrl}</a>) } },
      { id: 'docsurl', title: 'API Docs URL', val: p => { return (<a href={Urls.robiniadocsUrl(p.urlPrefix)} target="_blank">{Urls.robiniadocsUrl(p.urlPrefix)}</a>) } },
    ],
    data: result || []
  };

  return (
    <Grid container>
      <Grid item sm={2}></Grid>
      <Grid item sm={8}>
        <SimpleTable loading={loading} config={tconfig} />
      </Grid>
      <Grid item sm={2}></Grid>
    </Grid>
  );
}

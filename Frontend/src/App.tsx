import { Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Overview } from './pages/Overview';
import { FeaturePending } from './pages/FeaturePending';

export function App() {
  return <Routes><Route element={<Layout />}>
    <Route index element={<Overview />} />
    <Route path="documents" element={<FeaturePending title="documents" description="documentsBody" />} />
    <Route path="reminders" element={<FeaturePending title="reminders" description="remindersBody" />} />
    <Route path="shares" element={<FeaturePending title="shares" description="sharesBody" />} />
    <Route path="trash" element={<FeaturePending title="trash" description="trashBody" />} />
    <Route path="settings" element={<FeaturePending title="settings" description="settingsBody" />} />
    <Route path="*" element={<FeaturePending title="notFound" description="notFoundBody" />} />
  </Route></Routes>;
}

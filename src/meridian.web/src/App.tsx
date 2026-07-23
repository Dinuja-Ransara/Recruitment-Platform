import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, Roles, useAuth } from './auth/AuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AppShell } from './components/AppShell'
import { LoginPage } from './pages/LoginPage'
import { HomePage } from './pages/HomePage'

import { CandidateOverview } from './pages/candidate/CandidateOverview'
import { JobSearch } from './pages/candidate/JobSearch'
import { JobDetailPage } from './pages/candidate/JobDetailPage'
import { ApplicationDetailPage, MyApplications } from './pages/candidate/MyApplications'

import { RecruiterJobs } from './pages/recruiter/RecruiterJobs'
import { RankedApplicants } from './pages/recruiter/RankedApplicants'
import { JobForm } from './pages/recruiter/JobForm'

import { ManagerShortlists } from './pages/manager/ManagerShortlists'
import { OrganizationsPage, StaffOverview } from './pages/StaffOverview'

/**
 * Sends a signed-in user to the portal their role owns. A user holding several
 * roles lands on the most privileged one they have.
 */
function RoleLanding() {
  const { user, hasRole, isLoading } = useAuth()

  if (isLoading) return null
  if (!user) return <Navigate to="/login" replace />
  if (hasRole(Roles.Administrator)) return <Navigate to="/admin" replace />
  if (hasRole(Roles.Recruiter)) return <Navigate to="/recruiter" replace />
  if (hasRole(Roles.HiringManager)) return <Navigate to="/manager" replace />
  return <Navigate to="/candidate" replace />
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />

          <Route
            element={
              <ProtectedRoute>
                <AppShell />
              </ProtectedRoute>
            }
          >
            {/* Candidate portal */}
            <Route
              path="/candidate"
              element={
                <ProtectedRoute roles={[Roles.Candidate]}>
                  <CandidateOverview />
                </ProtectedRoute>
              }
            />
            <Route
              path="/candidate/jobs"
              element={
                <ProtectedRoute roles={[Roles.Candidate]}>
                  <JobSearch />
                </ProtectedRoute>
              }
            />
            <Route
              path="/candidate/jobs/:id"
              element={
                <ProtectedRoute roles={[Roles.Candidate]}>
                  <JobDetailPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/candidate/applications"
              element={
                <ProtectedRoute roles={[Roles.Candidate]}>
                  <MyApplications />
                </ProtectedRoute>
              }
            />
            <Route
              path="/candidate/applications/:id"
              element={
                <ProtectedRoute roles={[Roles.Candidate]}>
                  <ApplicationDetailPage />
                </ProtectedRoute>
              }
            />

            {/* Recruiter portal */}
            <Route
              path="/recruiter"
              element={
                <ProtectedRoute roles={[Roles.Recruiter]}>
                  <StaffOverview />
                </ProtectedRoute>
              }
            />
            <Route
              path="/recruiter/jobs"
              element={
                <ProtectedRoute roles={[Roles.Recruiter]}>
                  <RecruiterJobs />
                </ProtectedRoute>
              }
            />
            <Route
              path="/recruiter/jobs/new"
              element={
                <ProtectedRoute roles={[Roles.Recruiter]}>
                  <JobForm />
                </ProtectedRoute>
              }
            />
            <Route
              path="/recruiter/jobs/:id/applicants"
              element={
                <ProtectedRoute roles={[Roles.Recruiter, Roles.HiringManager, Roles.Administrator]}>
                  <RankedApplicants />
                </ProtectedRoute>
              }
            />

            {/* Hiring manager portal */}
            <Route
              path="/manager"
              element={
                <ProtectedRoute roles={[Roles.HiringManager]}>
                  <ManagerShortlists />
                </ProtectedRoute>
              }
            />

            {/* Administration portal */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute roles={[Roles.Administrator]}>
                  <StaffOverview />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/organizations"
              element={
                <ProtectedRoute roles={[Roles.Administrator]}>
                  <OrganizationsPage />
                </ProtectedRoute>
              }
            />
          </Route>

          <Route path="*" element={<RoleLanding />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

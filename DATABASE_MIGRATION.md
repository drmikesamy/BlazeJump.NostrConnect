# Database Migration Note

## New Migration Required

After these changes, you'll need to either:

### Option 1: Fresh Start (Recommended for Development)
1. Delete the app data (uninstall/reinstall or clear app data)
2. On first run, the database will be created with the new schema

### Option 2: Manual Migration (If preserving data)
The new `HealthData` table will be created automatically by EF Core's `EnsureCreatedAsync()`, but if you have existing profiles, no migration is needed - the app will work with existing data.

## Database Changes
- Added `HealthData` table with fields: Id, PublicKey, Type, Data, Timestamp, NostrEventId
- No changes to existing UserProfiles, Relays, or NostrConnectSessions tables

## Testing Checklist
- [ ] Create new profile
- [ ] Navigate to Overview (should show empty states)
- [ ] Add a vital sign
- [ ] Add a medication
- [ ] Add an appointment
- [ ] Verify Overview shows latest data
- [ ] Check that data appears in respective pages
- [ ] Verify Nostr events are posted (check logs)
- [ ] Restart app and verify profile loads
- [ ] Verify all data persists after restart

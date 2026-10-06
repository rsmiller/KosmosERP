import { test as setup, expect } from '../fixtures/test';
import { ok } from '../mocks/envelope';
import { users } from '../fixtures/users';
import { LoginPage } from '../pages/login-page';

// Log each persona in through the real login form once and save its storage
// state (the token, roles and name live in localStorage), so specs start
// authenticated without repeating the login flow.
for (const [name, persona] of Object.entries(users)) {
  setup(`authenticate as ${name}`, async ({ page, api }) => {
    api.on('POST', '/User/AuthenticateUser', ok(persona.auth));

    const login = new LoginPage(page);
    await login.goto();
    await login.login(persona.auth.user.username, persona.password);

    await expect(page).toHaveURL(/\/erp\/?$/);
    await page.context().storageState({ path: persona.storageState });
  });
}

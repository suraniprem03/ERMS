async function test() {
  try {
    const loginRes = await fetch('http://localhost:5091/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: 'superadmin@erms.local', password: 'ChangeMe@123' })
    });
    
    const loginData = await loginRes.json();
    console.log('Login Response:', loginData);

    if (!loginData.success) {
      console.log('Login failed');
      return;
    }

    const token = loginData.data.accessToken;
    console.log('Token extracted:', token ? (token.substring(0, 10) + '...') : 'undefined');

    const empRes = await fetch('http://localhost:5091/api/employees', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    
    console.log('Employees Status:', empRes.status);
    const empData = await empRes.text();
    console.log('Employees Body:', empData.substring(0, 200));

  } catch (err) {
    console.error(err);
  }
}

test();
